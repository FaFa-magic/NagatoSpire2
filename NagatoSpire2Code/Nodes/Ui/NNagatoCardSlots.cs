using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoCardSlots : Control
{
	private sealed class Entry(TextureRect bounds)
	{
		public TextureRect Bounds { get; } = bounds;
		public CardModel? Model;
		public NCard? Node;
	}

	private static readonly AccessTools.FieldRef<NOrbManager, NCreature> CreatureNode =
		AccessTools.FieldRefAccess<NOrbManager, NCreature>("_creatureNode");
	private static readonly AccessTools.FieldRef<NOrbManager, List<NOrb>> OrbNodes =
		AccessTools.FieldRefAccess<NOrbManager, List<NOrb>>("_orbs");
	private static readonly AccessTools.FieldRef<NOrbManager, Tween?> LayoutTween =
		AccessTools.FieldRefAccess<NOrbManager, Tween?>("_curTween");
	private readonly List<Entry> _entries = [];
	private NOrbManager _manager = null!;
	private Player _player = null!;
	private NCard? _preview;
	private Entry? _hovered;
	private bool _refresh = true;
	private Vector2 _center;
	private float _spacing;
	private NOrb[] _layoutNodes = [];
	private Vector2 _orbCenter;
	private float _orbSpacing;
	private static bool PreviewBlocked => NRun.Instance?.GlobalUi is { } ui &&
		(ui.TargetManager.IsInSelection || ui.Overlays.ScreenCount > 0 || ui.CapstoneContainer.InUse || ui.MapScreen.IsOpen);

	public static void Attach(NOrbManager manager)
	{
		if (manager.GetNodeOrNull<NNagatoCardSlots>("NagatoCardSlots") != null || CreatureNode(manager).Entity.Player is not { } player)
			return;
		manager.AddChild(new NNagatoCardSlots
		{
			Name = "NagatoCardSlots", MouseFilter = MouseFilterEnum.Ignore, _manager = manager, _player = player
		});
	}

	public static bool TryLayout(NOrbManager manager)
	{
		if (CreatureNode(manager).Entity.Player is not { } player || NagatoCardSlots.GetCapacity(player) == 0)
			return false;
		Attach(manager);
		manager.GetNode<NNagatoCardSlots>("NagatoCardSlots").Layout();
		return true;
	}

	public static void RemoveHandVisual(CardModel card)
	{
		var node = NCard.FindOnTable(card);
		if (LocalContext.IsMe(card.Owner) && NCombatRoom.Instance?.Ui?.Hand is { } hand && hand.GetCardHolder(card) != null)
			hand.Remove(card);
		if (GodotObject.IsInstanceValid(node))
			node!.QueueFreeSafely();
	}

	public override void _EnterTree() => CombatManager.Instance.StateTracker.CombatStateChanged += Changed;
	private void Changed(CombatState _) => _refresh = true;

	public override void _Process(double delta)
	{
		Visible = NagatoCardSlots.GetCapacity(_player) > 0 && !_player.Creature.IsDead && !CombatManager.Instance.IsOverOrEnding;
		if (!Visible)
		{
			ClearPreview();
			return;
		}
		Layout();
		RefreshCards();
		if (_hovered != null && PreviewBlocked)
			ClearPreview();
	}

	private void Layout()
	{
		var creature = CreatureNode(_manager);
		var viewport = GetViewportRect();
		Vector2 globalScale = _manager.GetGlobalTransform().Scale;
		int count = Math.Max(NagatoCardSlots.GetCapacity(_player), _player.PlayerCombatState?.OrbQueue.Capacity ?? 0);
		_spacing = _manager.IsLocal ? 72f : 54f;
		Vector2 top = creature.GetTopOfHitbox();
		foreach (var player in _player.RunState.Players.Where(player => player != _player && NagatoCardSlots.GetCapacity(player) > 0))
		{
			if (NCombatRoom.Instance?.GetCreatureNode(player.Creature) is not { } other)
				continue;
			Vector2 otherTop = other.GetTopOfHitbox();
			if (Mathf.Abs(top.Y - otherTop.Y) < 120f && Mathf.Abs(top.X - otherTop.X) > 1f)
				_spacing = Mathf.Min(_spacing, Mathf.Abs(top.X - otherTop.X) * 0.85f / (count * globalScale.X));
		}
		_spacing = Mathf.Max(24f, _spacing);
		float width = count * _spacing * globalScale.X;
		top.X = Mathf.Clamp(top.X, viewport.Position.X + width * 0.5f + 12f, viewport.End.X - width * 0.5f - 12f);
		top.Y = Mathf.Max(top.Y - 110f * globalScale.Y, viewport.Position.Y + 185f);
		_center = _manager.GetGlobalTransform().AffineInverse() * top;
		var nodes = OrbNodes(_manager);
		Vector2 orbCenter = _center + new Vector2(0f, -52f);
		if (_layoutNodes.SequenceEqual(nodes) && _orbCenter.IsEqualApprox(orbCenter) && Mathf.IsEqualApprox(_orbSpacing, _spacing))
			return;
		_layoutNodes = nodes.ToArray();
		_orbCenter = orbCenter;
		_orbSpacing = _spacing;
		_manager.GetNode<Control>("%Orbs").Position = Vector2.Zero;
		LayoutTween(_manager)?.Kill();
		LayoutTween(_manager) = null;
		if (nodes.Count == 0)
			return;
		var tween = _manager.CreateTween().SetParallel();
		LayoutTween(_manager) = tween;
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].Scale = Vector2.One * Mathf.Min(_manager.IsLocal ? 0.65f : 0.5f, _spacing / 80f);
			tween.TweenProperty(nodes[i], "position", orbCenter + Vector2.Left * ((i - (nodes.Count - 1) * 0.5f) * _spacing), 0.25)
				.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		}
	}

	private void RefreshCards()
	{
		int capacity = NagatoCardSlots.GetCapacity(_player);
		while (_entries.Count < capacity)
		{
			var bounds = new TextureRect
			{
				Name = $"CardSlot{_entries.Count + 1}",
				Texture = ResourceLoader.Load<Texture2D>("res://NagatoSpire2/images/ui/card_slot_outline.svg"),
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				MouseFilter = MouseFilterEnum.Pass, FocusMode = FocusModeEnum.None
			};
			var entry = new Entry(bounds);
			bounds.MouseEntered += () => ShowPreview(entry);
			bounds.MouseExited += () => { if (_hovered == entry) ClearPreview(); };
			_entries.Add(entry);
			AddChild(bounds);
		}
		var cards = NagatoCardSlots.GetCards(_player);
		float scale = Mathf.Min(_manager.IsLocal ? 0.2f : 0.16f, (_spacing - 6f) / NCard.defaultSize.X);
		Vector2 size = NCard.defaultSize * scale;
		for (int i = 0; i < _entries.Count; i++)
		{
			var entry = _entries[i];
			CardModel? model = i < cards.Count ? cards[i] : null;
			if (entry.Model != model)
			{
				if (_hovered == entry)
					ClearPreview();
				entry.Node?.QueueFreeSafely();
				entry.Model = model;
				entry.Node = model != null ? CreateCard(model) : null;
				if (entry.Node != null)
					entry.Bounds.AddChild(entry.Node);
				_refresh = true;
			}
			entry.Bounds.Size = size;
			entry.Bounds.Position = _center + Vector2.Left * ((i - (capacity - 1) * 0.5f) * _spacing) - new Vector2(size.X * 0.5f, 0f);
			entry.Bounds.SelfModulate = model == null ? Colors.White : Colors.Transparent;
			entry.Bounds.MouseFilter = PreviewBlocked ? MouseFilterEnum.Ignore : MouseFilterEnum.Pass;
			if (entry.Node is not { } card)
				continue;
			card.Scale = Vector2.One * scale;
			card.Position = size * 0.5f;
			if (_refresh)
				card.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
		}
		if (_refresh && _preview != null)
			_preview.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
		_refresh = false;
	}

	private static NCard CreateCard(CardModel model)
	{
		var node = PreloadManager.Cache.GetScene("res://scenes/cards/card.tscn").Instantiate<NCard>();
		node.Model = model;
		node.MouseFilter = MouseFilterEnum.Ignore;
		foreach (var control in node.FindChildren("*", "Control", true, false).OfType<Control>())
			control.MouseFilter = MouseFilterEnum.Ignore;
		return node;
	}

	private void ShowPreview(Entry entry)
	{
		ClearPreview();
		if (entry.Model is not { } model || PreviewBlocked)
			return;
		_hovered = entry;
		_preview = CreateCard(model);
		_preview.TopLevel = true;
		_preview.ZIndex = 100;
		_preview.Scale = Vector2.One * 0.65f;
		AddChild(_preview);
		Vector2 size = NCard.defaultSize * _preview.Scale;
		var viewport = GetViewportRect();
		Vector2 position = entry.Bounds.GlobalPosition + new Vector2(entry.Bounds.Size.X * 0.5f, -size.Y * 0.5f - 20f);
		_preview.GlobalPosition = new Vector2(
			Mathf.Clamp(position.X, viewport.Position.X + size.X * 0.5f + 12f, viewport.End.X - size.X * 0.5f - 12f),
			Mathf.Clamp(position.Y, viewport.Position.Y + size.Y * 0.5f + 100f, viewport.End.Y - size.Y * 0.5f - 12f));
		_preview.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
		NHoverTipSet.CreateAndShow(_preview, model.HoverTips);
	}

	private void ClearPreview()
	{
		if (GodotObject.IsInstanceValid(_preview))
		{
			NHoverTipSet.Remove(_preview!);
			_preview!.QueueFreeSafely();
		}
		_preview = null;
		_hovered = null;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= Changed;
		ClearPreview();
	}
}
