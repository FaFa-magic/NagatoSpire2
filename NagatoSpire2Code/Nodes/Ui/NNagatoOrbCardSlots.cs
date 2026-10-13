using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoOrbCardSlots : Control
{
	private sealed class Entry(Panel bounds)
	{
		public Panel Bounds { get; } = bounds;
		public CardModel? Model;
		public NCard? Node;
		public Action? Refresh;
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
	private float _spacing;
	private bool _refreshRequested;
	private NOrb[] _layoutNodes = [];
	private Vector2 _layoutCenter;
	private float _layoutSpacing;
	private int _layoutCount;
	private static bool IsTargeting => NRun.Instance?.GlobalUi?.TargetManager?.IsInSelection == true;
	private static bool IsPreviewBlocked => IsTargeting ||
		NRun.Instance?.GlobalUi?.Overlays?.ScreenCount > 0 ||
		NRun.Instance?.GlobalUi?.CapstoneContainer?.InUse == true ||
		NRun.Instance?.GlobalUi?.MapScreen?.IsOpen == true;

	public static bool TryLayout(NOrbManager manager)
	{
		if (CreatureNode(manager).Entity.Player is not { } player || !NagatoOrbCardSlots.IsEnabled(player) ||
			player.PlayerCombatState == null)
			return false;
		var view = manager.GetNodeOrNull<NNagatoOrbCardSlots>("NagatoOrbCardSlots");
		if (view == null)
		{
			view = new NNagatoOrbCardSlots
			{
				Name = "NagatoOrbCardSlots", MouseFilter = MouseFilterEnum.Ignore,
				_manager = manager, _player = player
			};
			manager.AddChild(view);
		}
		view.LayoutOrbs();
		view.RefreshSlots();
		return true;
	}

	private void LayoutOrbs()
	{
		var creature = CreatureNode(_manager);
		var nodes = OrbNodes(_manager);
		SyncOrbNodes(nodes);
		int count = _player.PlayerCombatState!.OrbQueue.Capacity;
		var viewport = GetViewportRect();
		Vector2 top = creature.GetTopOfHitbox();
		Vector2 globalScale = _manager.GetGlobalTransform().Scale;
		_spacing = _manager.IsLocal ? 52f : 40f;
		foreach (var player in _player.RunState.Players.Where(player => player != _player))
		{
			var other = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
			if (other == null || !NagatoOrbCardSlots.IsEnabled(player))
				continue;
			Vector2 otherTop = other.GetTopOfHitbox();
			if (Mathf.Abs(top.Y - otherTop.Y) < 90f && Mathf.Abs(top.X - otherTop.X) > 1f)
				_spacing = Mathf.Min(_spacing, Mathf.Abs(top.X - otherTop.X) * 0.9f / (count * globalScale.X));
		}
		_spacing = Mathf.Max(18f, _spacing);
		float width = _spacing * count * globalScale.X;
		top.X = Mathf.Clamp(top.X, viewport.Position.X + width * 0.5f + 12f, viewport.End.X - width * 0.5f - 12f);
		top.Y = Mathf.Max(top.Y - 110f * globalScale.Y, viewport.Position.Y + 180f);
		var others = _player.RunState.Players.Where(player => player != _player)
			.Select(player => NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.Hitbox.GetGlobalRect())
			.OfType<Rect2>().ToArray();
		bool Fits(Vector2 center) => !others.Any(bounds => bounds.Intersects(
			new Rect2(center - new Vector2(width * 0.5f, 24f), new Vector2(width, 100f * globalScale.Y))));
		if (!Fits(top))
		{
			Rect2 bounds = creature.Hitbox.GetGlobalRect();
			var candidates = new[]
			{
				new Vector2(bounds.Position.X - width * 0.5f - 12f, top.Y),
				new Vector2(bounds.End.X + width * 0.5f + 12f, top.Y)
			};
			var candidate = candidates.Where(point => point.X - width * 0.5f >= viewport.Position.X + 12f &&
				point.X + width * 0.5f <= viewport.End.X - 12f && Fits(point))
				.OrderBy(point => point.DistanceSquaredTo(top)).Cast<Vector2?>().FirstOrDefault();
			if (candidate is { } position)
				top = position;
		}
		Vector2 center = _manager.GetGlobalTransform().AffineInverse() * top;
		if (_layoutCount == count && _layoutNodes.SequenceEqual(nodes) &&
			_layoutCenter.IsEqualApprox(center) && Mathf.IsEqualApprox(_layoutSpacing, _spacing))
			return;
		_layoutCount = count;
		_layoutNodes = nodes.ToArray();
		_layoutCenter = center;
		_layoutSpacing = _spacing;
		_manager.GetNode<Control>("%Orbs").Position = Vector2.Zero;
		LayoutTween(_manager)?.Kill();
		if (nodes.Count == 0)
		{
			LayoutTween(_manager) = null;
			return;
		}
		var tween = _manager.CreateTween().SetParallel();
		LayoutTween(_manager) = tween;
		float scale = Mathf.Min(_manager.IsLocal ? 0.65f : 0.55f, _spacing / 72f);
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].Scale = Vector2.One * scale;
			tween.TweenProperty(nodes[i], "position", SlotPosition(i), 0.25)
				.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		}
	}

	private void SyncOrbNodes(List<NOrb> nodes)
	{
		var slots = NagatoOrbBindings.GetSlots(_player);
		if (slots.Length != nodes.Count || nodes.Select(node => node.Model).SequenceEqual(slots))
			return;
		var remaining = nodes.ToList();
		var ordered = new List<NOrb>(slots.Length);
		var container = _manager.GetNode<Control>("%Orbs");
		foreach (var model in slots)
		{
			var node = remaining.FirstOrDefault(node => node.Model == model);
			if (node != null)
				remaining.Remove(node);
			else
			{
				node = NOrb.Create(_manager.IsLocal, model);
				container.AddChild(node);
				node.Position = _layoutCount > 0 ? SlotPosition(ordered.Count) : Vector2.Zero;
			}
			ordered.Add(node);
		}
		foreach (var node in remaining)
			node.QueueFree();
		nodes.Clear();
		nodes.AddRange(ordered);
	}

	private Vector2 SlotPosition(int index) => _layoutCenter + Vector2.Left *
		((index - (_player.PlayerCombatState!.OrbQueue.Capacity - 1) * 0.5f) * _spacing);

	public override void _EnterTree() => CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;

	private void OnCombatStateChanged(CombatState _) => _refreshRequested = true;

	public override void _Process(double delta)
	{
		if (!NagatoOrbCardSlots.IsEnabled(_player) || _player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
		{
			ClearPreview();
			foreach (var entry in _entries)
			{
				Unbind(entry);
				entry.Bounds.QueueFree();
			}
			_entries.Clear();
			Hide();
			return;
		}
		Show();
		LayoutOrbs();
		RefreshSlots();
		if (_hovered != null && IsPreviewBlocked)
			ClearPreview();
	}

	private void RefreshSlots()
	{
		var cards = NagatoOrbCardSlots.GetCards(_player);
		while (_entries.Count > cards.Length)
		{
			var entry = _entries[^1];
			Unbind(entry);
			entry.Bounds.QueueFree();
			_entries.RemoveAt(_entries.Count - 1);
		}
		while (_entries.Count < cards.Length)
		{
			var bounds = new Panel { MouseFilter = MouseFilterEnum.Pass, FocusMode = FocusModeEnum.None };
			bounds.AddThemeStyleboxOverride("panel", new StyleBoxFlat
			{
				BgColor = new Color("#34242c88"), BorderColor = new Color("#d4a7b477"),
				BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
				CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3
			});
			var entry = new Entry(bounds);
			bounds.MouseEntered += () => ShowPreview(entry);
			bounds.MouseExited += () => { if (_hovered == entry) ClearPreview(); };
			_entries.Add(entry);
			AddChild(bounds);
		}
		float cardScale = Mathf.Min(_manager.IsLocal ? 0.16f : 0.12f, (_spacing - 4f) / NCard.defaultSize.X);
		Vector2 size = NCard.defaultSize * cardScale;
		for (int i = 0; i < cards.Length; i++)
		{
			var entry = _entries[i];
			if (entry.Model != cards[i])
				Bind(entry, cards[i]);
			entry.Bounds.Size = size;
			entry.Bounds.Position = SlotPosition(i) + new Vector2(-size.X * 0.5f, 26f);
			entry.Bounds.SelfModulate = entry.Model == null ? Colors.White : Colors.Transparent;
			entry.Bounds.MouseFilter = IsTargeting ? MouseFilterEnum.Ignore : MouseFilterEnum.Pass;
			if (entry.Node is { } card)
			{
				card.Scale = Vector2.One * cardScale;
				card.Position = size * 0.5f;
				if (_refreshRequested)
					card.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
			}
		}
		if (_refreshRequested && _preview != null)
			_preview.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
		_refreshRequested = false;
	}

	private static NCard CreateCard(CardModel model)
	{
		var node = PreloadManager.Cache.GetScene("res://scenes/cards/card.tscn").Instantiate<NCard>();
		node.Model = model;
		node.MouseFilter = MouseFilterEnum.Ignore;
		return node;
	}

	private void Bind(Entry entry, CardModel? model)
	{
		Unbind(entry);
		entry.Model = model;
		if (model == null)
			return;
		entry.Node = CreateCard(model);
		entry.Bounds.AddChild(entry.Node);
		foreach (var control in entry.Node.FindChildren("*", "Control", true, false).OfType<Control>())
			control.MouseFilter = MouseFilterEnum.Ignore;
		entry.Node.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
		entry.Refresh = () => _refreshRequested = true;
		model.AfflictionChanged += entry.Refresh;
		model.EnchantmentChanged += entry.Refresh;
		model.EnergyCostChanged += entry.Refresh;
		model.KeywordsChanged += entry.Refresh;
		model.ReplayCountChanged += entry.Refresh;
		model.StarCostChanged += entry.Refresh;
		model.Upgraded += entry.Refresh;
		model.Forged += entry.Refresh;
	}

	private void Unbind(Entry entry)
	{
		if (_hovered == entry)
			ClearPreview();
		if (entry.Model is { } model)
		{
			model.AfflictionChanged -= entry.Refresh;
			model.EnchantmentChanged -= entry.Refresh;
			model.EnergyCostChanged -= entry.Refresh;
			model.KeywordsChanged -= entry.Refresh;
			model.ReplayCountChanged -= entry.Refresh;
			model.StarCostChanged -= entry.Refresh;
			model.Upgraded -= entry.Refresh;
			model.Forged -= entry.Refresh;
		}
		entry.Refresh = null;
		entry.Model = null;
		if (GodotObject.IsInstanceValid(entry.Node))
			entry.Node!.QueueFree();
		entry.Node = null;
	}

	private void ShowPreview(Entry entry)
	{
		ClearPreview();
		if (entry.Model is not { } model || IsPreviewBlocked)
			return;
		_hovered = entry;
		_preview = CreateCard(model);
		_preview.TopLevel = true;
		_preview.ZIndex = 100;
		_preview.Scale = Vector2.One * 0.65f;
		AddChild(_preview);
		foreach (var control in _preview.FindChildren("*", "Control", true, false).OfType<Control>())
			control.MouseFilter = MouseFilterEnum.Ignore;
		var viewport = GetViewportRect();
		Vector2 size = NCard.defaultSize * _preview.Scale;
		Vector2 position = entry.Bounds.GlobalPosition + new Vector2(entry.Bounds.Size.X * 0.5f, -size.Y * 0.5f - 20f);
		_preview.GlobalPosition = new Vector2(
			Mathf.Clamp(position.X, viewport.Position.X + size.X * 0.5f + 12f, viewport.End.X - size.X * 0.5f - 12f),
			Mathf.Clamp(position.Y, viewport.Position.Y + size.Y * 0.5f + 100f, viewport.End.Y - size.Y * 0.5f - 12f));
		_preview.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
	}

	private void ClearPreview()
	{
		if (GodotObject.IsInstanceValid(_preview))
			_preview!.QueueFree();
		_preview = null;
		_hovered = null;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		ClearPreview();
		foreach (var entry in _entries)
			Unbind(entry);
		_entries.Clear();
	}
}
