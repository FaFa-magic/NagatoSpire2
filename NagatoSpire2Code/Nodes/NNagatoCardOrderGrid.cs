using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Nodes;

public partial class NNagatoCardOrderGrid : Control
{
	private readonly List<CardModel> _cards = [];
	private readonly List<NGridCardHolder> _holders = [];
	private readonly List<Panel> _markers = [];
	private readonly List<(Callable Focused, Callable Unfocused)> _markerSignals = [];
	private readonly NagatoCardOrderHistory<CardModel> _history = new();
	private ScrollContainer _scroll = null!;
	private GridContainer _slots = null!;
	private int? _selected;
	private int? _draggedIndex;
	private ulong _suppressPressUntil;

	public Func<bool>? CanInteract { get; set; }
	public Control? NavigationFooter { get; set; }
	public Control? NavigationSidebar { get; set; }
	public IReadOnlyList<CardModel> OrderedCards => _cards;
	public event Action? OrderChanged;
	public bool CanUndo => _history.CanUndo;
	public bool IsOriginal => _history.IsOriginal(_cards);
	public int? SelectedIndex => _selected;
	public bool CanEdit => Interactive && !IsDragging;
	public Control? DefaultFocusedControl => _holders.FirstOrDefault();
	private bool Interactive => IsVisibleInTree() && (CanInteract?.Invoke() ?? true);
	private bool IsDragging => _draggedIndex.HasValue;

	public override void _Ready()
	{
		_scroll = GetNode<ScrollContainer>("Scroll");
		_slots = GetNode<GridContainer>("Scroll/Slots");
		_slots.Columns = 5;
	}

	public void SetCards(IEnumerable<CardModel> cards)
	{
		if (_holders.Count > 0)
			throw new InvalidOperationException("A card-order grid can only be initialized once.");
		_cards.AddRange(cards);
		_history.Initialize(_cards);
		for (int index = 0; index < _cards.Count; index++)
			CreateSlot(index);
		UpdateNavigation();
		OrderChanged?.Invoke();
	}

	public void Undo()
	{
		if (!Interactive || IsDragging || !CanUndo) return;
		_cards.Clear();
		_cards.AddRange(_history.Undo());
		RefreshCards();
	}

	public void ResetOrder()
	{
		if (!Interactive || IsDragging || IsOriginal) return;
		_history.Remember(_cards);
		_cards.Clear();
		_cards.AddRange(_history.Original);
		RefreshCards();
	}

	private void CreateSlot(int index)
	{
		var slot = new Control
		{
			Name = $"Position{index + 1}",
			CustomMinimumSize = new Vector2(280, 466),
			MouseFilter = MouseFilterEnum.Pass
		};
		_slots.AddChild(slot);
		var marker = new Panel
		{
			Name = "DeploymentMarker",
			Position = new Vector2(8, 10),
			Size = new Vector2(264, 448),
			MouseFilter = MouseFilterEnum.Ignore
		};
		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.18f, 0.12f, 0.08f, 0.055f),
			BorderColor = new Color("96764a88"),
			CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5,
			CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5
		};
		style.SetBorderWidthAll(1);
		marker.AddThemeStyleboxOverride("panel", style);
		slot.AddChild(marker);
		_markers.Add(marker);
		var ordinal = new Label
		{
			Name = "DrawOrder", Text = index == 0
				? "01 · " + new LocString("gameplay_ui", "NAGATO_ORDER.first").GetFormattedText()
				: (index + 1).ToString("00"),
			Position = new Vector2(10, 430), Size = new Vector2(260, 28),
			HorizontalAlignment = HorizontalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		ordinal.AddThemeFontSizeOverride("font_size", 20);
		ordinal.AddThemeColorOverride("font_color", new Color("654524"));
		ordinal.AddThemeColorOverride("font_shadow_color", Colors.Transparent);
		slot.AddChild(ordinal);
		var anchor = new Control
		{
			Name = "CardAnchor",
			Position = new Vector2(140, NCard.defaultSize.Y * 0.5f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		slot.AddChild(anchor);
		NCard card = NCard.Create(_cards[index]) ?? throw new InvalidOperationException("Could not create the card-order preview.");
		NGridCardHolder holder = NGridCardHolder.Create(card) ?? throw new InvalidOperationException("Could not create the card-order holder.");
		_holders.Add(holder);
		holder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(OnHolderPressed));
		holder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(OnHolderAltPressed));
		anchor.AddChildSafely(holder);
		holder.Scale = holder.SmallScale;
		holder.CardNode!.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
		Callable.From(() => ConfigureDrag(index, holder, slot)).CallDeferred();
		var focused = Callable.From<NClickableControl>(_ => SetMarker(index, true));
		var unfocused = Callable.From<NClickableControl>(_ => SetMarker(index, false));
		holder.Hitbox.Connect(NClickableControl.SignalName.Focused, focused);
		holder.Hitbox.Connect(NClickableControl.SignalName.Unfocused, unfocused);
		_markerSignals.Add((focused, unfocused));
	}

	private void SetMarker(int index, bool hovered)
	{
		var style = (StyleBoxFlat)_markers[index].GetThemeStylebox("panel");
		style.BorderColor = _selected == index ? new Color("b14d32") : hovered ? new Color("bc914a") : new Color("96764a88");
		style.BgColor = _selected == index ? new Color(0.66f, 0.22f, 0.12f, 0.09f) : new Color(0.18f, 0.12f, 0.08f, 0.055f);
		style.SetBorderWidthAll(_selected == index || hovered ? 2 : 1);
	}

	private void ConfigureDrag(int index, NGridCardHolder holder, Control slot)
	{
		if (!IsInsideTree() || !GodotObject.IsInstanceValid(holder))
			return;
		holder.Hitbox.MouseDefaultCursorShape = CursorShape.Drag;
		holder.Hitbox.SetDragForwarding(
			Callable.From<Vector2, Variant>(position => BeginDrag(index, holder.Hitbox)),
			Callable.From<Vector2, Variant, bool>((position, data) => CanDrop(data)),
			Callable.From<Vector2, Variant>((position, data) => Drop(index, data)));
		slot.SetDragForwarding(
			Callable.From<Vector2, Variant>(position => BeginDrag(index, slot)),
			Callable.From<Vector2, Variant, bool>((position, data) => CanDrop(data)),
			Callable.From<Vector2, Variant>((position, data) => Drop(index, data)));
	}

	private Variant BeginDrag(int index, Control source)
	{
		if (!Interactive || index >= _cards.Count)
			return default;
		NCard card = NCard.Create(_cards[index]) ?? throw new InvalidOperationException("Could not create the dragged card preview.");
		var preview = new Control { MouseFilter = MouseFilterEnum.Ignore };
		card.Scale = _holders[index].Scale;
		card.MouseFilter = MouseFilterEnum.Ignore;
		preview.AddChild(card);
		preview.TreeExiting += () =>
		{
			if (GodotObject.IsInstanceValid(card))
				card.QueueFreeSafely();
		};
		source.SetDragPreview(preview);
		card.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
		_draggedIndex = index;
		OrderChanged?.Invoke();
		_holders[index].Modulate = new Color(1f, 1f, 1f, 0.3f);
		_selected = null;
		RefreshSelection();
		NHoverTipSet.Clear();
		return new Godot.Collections.Dictionary { ["grid"] = GetInstanceId(), ["index"] = index };
	}

	private bool CanDrop(Variant data)
	{
		if (!Interactive || data.VariantType != Variant.Type.Dictionary)
			return false;
		var payload = data.AsGodotDictionary();
		return payload.ContainsKey("grid") && payload.ContainsKey("index") &&
			payload["grid"].AsUInt64() == GetInstanceId() &&
			payload["index"].AsInt32() >= 0 && payload["index"].AsInt32() < _cards.Count;
	}

	private void Drop(int to, Variant data)
	{
		if (!CanDrop(data))
			return;
		int from = data.AsGodotDictionary()["index"].AsInt32();
		MoveCard(from, to);
		_suppressPressUntil = Time.GetTicksMsec() + 150;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationDragEnd)
		{
			if (_draggedIndex is int index && index < _holders.Count)
				_holders[index].Modulate = Colors.White;
			_draggedIndex = null;
			_suppressPressUntil = Time.GetTicksMsec() + 150;
			OrderChanged?.Invoke();
		}
	}

	public override void _Process(double delta)
	{
		if (!IsDragging || !Interactive)
			return;
		Rect2 bounds = _scroll.GetGlobalRect();
		Vector2 mouse = GetGlobalMousePosition();
		if (mouse.X < bounds.Position.X || mouse.X > bounds.End.X)
			return;
		int speed = mouse.Y < bounds.Position.Y + 64 ? -1 : mouse.Y > bounds.End.Y - 64 ? 1 : 0;
		_scroll.ScrollVertical += (int)(speed * 650 * delta);
	}

	private void OnHolderPressed(NCardHolder holder)
	{
		if (!Interactive || IsDragging || Time.GetTicksMsec() < _suppressPressUntil)
			return;
		int index = _holders.IndexOf((NGridCardHolder)holder);
		if (index < 0)
			return;
		if (_selected is int from)
		{
			if (from != index) _history.Remember(_cards);
			(_cards[from], _cards[index]) = (_cards[index], _cards[from]);
			RefreshCards();
		}
		else
		{
			_selected = index;
			RefreshSelection();
		}
	}

	private void OnHolderAltPressed(NCardHolder holder)
	{
		if (!Interactive || IsDragging)
			return;
		int index = _holders.IndexOf((NGridCardHolder)holder);
		if (index >= 0)
			NGame.Instance?.GetInspectCardScreen().Open(_cards.ToList(), index);
	}

	private void MoveCard(int from, int to)
	{
		if (from != to) _history.Remember(_cards);
		NagatoCardOrder.Move(_cards, from, to);
		RefreshCards();
	}

	private void RefreshCards()
	{
		_selected = null;
		NHoverTipSet.Clear();
		for (int i = 0; i < _holders.Count; i++)
			_holders[i].ReassignToCard(_cards[i], PileType.Draw, null, ModelVisibility.Visible);
		RefreshSelection();
	}

	private void RefreshSelection()
	{
		for (int i = 0; i < _holders.Count; i++)
		{
			var highlight = _holders[i].CardNode!.CardHighlight;
			if (_selected == i)
				highlight.AnimShow();
			else
				highlight.AnimHideInstantly();
			SetMarker(i, _holders[i].Hitbox.HasFocus());
		}
		OrderChanged?.Invoke();
	}

	private void UpdateNavigation()
	{
		for (int i = 0; i < _holders.Count; i++)
		{
			NGridCardHolder holder = _holders[i];
			Control At(int index) => index >= 0 && index < _holders.Count ? _holders[index] : NavigationFooter ?? holder;
			holder.FocusNeighborLeft = (i % _slots.Columns == 0 ? NavigationSidebar ?? holder : At(i - 1)).GetPath();
			holder.FocusNeighborRight = At((i + 1) % _slots.Columns == 0 ? i : i + 1).GetPath();
			holder.FocusNeighborTop = At(i < _slots.Columns ? i : i - _slots.Columns).GetPath();
			holder.FocusNeighborBottom = At(i + _slots.Columns).GetPath();
		}
		if (NavigationFooter != null && _holders.Count > 0)
			NavigationFooter.FocusNeighborTop = _holders[^1].GetPath();
		if (NavigationSidebar != null && _holders.Count > 0)
			NavigationSidebar.FocusNeighborRight = _holders[0].GetPath();
	}

	public override void _ExitTree()
	{
		for (int i = 0; i < _holders.Count; i++)
		{
			_holders[i].Hitbox.Disconnect(NClickableControl.SignalName.Focused, _markerSignals[i].Focused);
			_holders[i].Hitbox.Disconnect(NClickableControl.SignalName.Unfocused, _markerSignals[i].Unfocused);
		}
		foreach (NGridCardHolder holder in _holders)
		{
			holder.Hitbox.SetDragForwarding(default, default, default);
			holder.Disconnect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(OnHolderPressed));
			holder.Disconnect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(OnHolderAltPressed));
			holder.QueueFreeSafely();
		}
		_holders.Clear();
		_markerSignals.Clear();
		_markers.Clear();
	}
}
