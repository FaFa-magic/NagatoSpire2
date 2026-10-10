using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

namespace NagatoSpire2.NagatoSpire2Code.Nodes;

public partial class NNagatoCardOrderScreen : Control, IOverlayScreen
{
	public const string ScenePath = "res://NagatoSpire2/scenes/screens/card_order_screen.tscn";
	private readonly TaskCompletionSource<CardModel[]> _completion = new();
	private CardModel[] _original = [];
	private LocString _prompt = null!;
	private NNagatoCardOrderGrid _grid = null!;
	private NConfirmButton _confirm = null!;
	private NPeekButton _peekButton = null!;
	private NagatoOrderTable _table = null!;
	private NNagatoOrderActionButton _undo = null!;
	private NNagatoOrderActionButton _reset = null!;
	private bool _closed;

	public NetScreenType ScreenType => NetScreenType.CardSelection;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => _peekButton?.IsPeeking == true
		? NCombatRoom.Instance?.DefaultFocusedControl ?? _peekButton
		: _grid?.DefaultFocusedControl ?? _confirm;
	public Control? FocusedControlFromTopBar => _peekButton?.IsPeeking == true
		? NCombatRoom.Instance?.FocusedControlFromTopBar ?? _peekButton
		: DefaultFocusedControl;
	private bool CanInteract => !_closed && !_peekButton.IsPeeking && IsVisibleInTree() && ActiveScreenContext.Instance.IsCurrent(this);

	public static NNagatoCardOrderScreen Create(CardModel[] cards, LocString prompt)
	{
		var screen = ResourceLoader.Load<PackedScene>(ScenePath).Instantiate<NNagatoCardOrderScreen>();
		screen._original = cards.ToArray();
		screen._prompt = prompt;
		return screen;
	}

	public override void _Ready()
	{
		_confirm = GetNode<NConfirmButton>("Confirm");
		_peekButton = GetNode<NPeekButton>("PeekButton");
		_table = GetNode<NagatoOrderTable>("Table");
		_table.Configure(Text("guide"), _prompt.GetFormattedText() + "\n" + Text("detail"), Text("undo"), Text("reset"));
		StyleConfirm();
		_undo = NNagatoOrderActionButton.Create(_table.UndoVisual, _table.Design);
		_reset = NNagatoOrderActionButton.Create(_table.ResetVisual, _table.Design);
		_undo.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUndo));
		_reset.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnReset));
		_grid = ResourceLoader.Load<PackedScene>("res://NagatoSpire2/scenes/cards/card_order_grid.tscn").Instantiate<NNagatoCardOrderGrid>();
		_grid.SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft);
		_grid.Position = new Vector2(260, 180);
		_grid.Size = new Vector2(1500, 610);
		_table.Design.AddChild(_grid);
		_confirm.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnConfirm));
		_peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(OnPeekToggled));
		_peekButton.AddTargets(_table, _confirm);
		_grid.CanInteract = () => CanInteract;
		_grid.NavigationFooter = _confirm;
		_grid.NavigationSidebar = _peekButton;
		_grid.OrderChanged += UpdateStatus;
		_grid.SetCards(_original);
		ActiveScreenContext.Instance.Updated += OnContextUpdated;
	}

	private void OnContextUpdated() { if (IsInsideTree() && _grid != null) UpdateStatus(); }

	private static string Text(string key) => new LocString("gameplay_ui", "NAGATO_ORDER." + key).GetFormattedText();

	private void StyleConfirm()
	{
		// Skin this instance only; retain the stock confirm hotkey, click and animation code.
		_confirm.Theme = _table.Theme;
		foreach (string name in new[] { "Image", "Outline", "Shadow" })
		{
			var image = _confirm.GetNode<TextureRect>(name);
			image.Texture = _table.CommandPlateTexture;
			image.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			image.StretchMode = TextureRect.StretchModeEnum.Scale;
			image.Position = name == "Outline" ? new Vector2(-83, 7) : name == "Shadow" ? new Vector2(-76, 17) : new Vector2(-80, 10);
			image.Size = name == "Outline" ? new Vector2(230, 76) : new Vector2(224, 70);
		}
		var face = _confirm.GetNode<TextureRect>("Image");
		face.GetNode<Control>("Icon").Hide();
		face.GetNode<Control>("HotkeyIcon").Position = new Vector2(167, 65);
		var label = new Label
		{
			Name = "DeploymentText", Text = Text("confirm"),
			HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		face.AddChild(label);
		label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		label.AddThemeFontSizeOverride("font_size", 25);
	}

	private void OnUndo(NButton button) { if (CanInteract) _grid.Undo(); }
	private void OnReset(NButton button) { if (CanInteract) _grid.ResetOrder(); }

	private void UpdateStatus()
	{
		var message = new LocString("gameplay_ui", _grid.SelectedIndex.HasValue ? "NAGATO_ORDER.selected" : "NAGATO_ORDER.status");
		message.Add("Count", _grid.OrderedCards.Count);
		message.Add("Position", (_grid.SelectedIndex ?? 0) + 1);
		message.Add("State", Text(_grid.IsOriginal ? "original" : "changed"));
		_table.SetStatus(message.GetFormattedText());
		_undo.SetEnabled(_grid.CanEdit && _grid.CanUndo);
		_reset.SetEnabled(_grid.CanEdit && !_grid.IsOriginal);
		Control first = _grid.DefaultFocusedControl ?? _confirm;
		_undo.FocusNeighborTop = _reset.FocusNeighborTop = first.GetPath();
		_undo.FocusNeighborLeft = first.GetPath();
		_undo.FocusNeighborRight = (_reset.IsEnabled ? (Control)_reset : _confirm).GetPath();
		_reset.FocusNeighborLeft = (_undo.IsEnabled ? (Control)_undo : first).GetPath();
		_reset.FocusNeighborRight = _confirm.GetPath();
		_confirm.FocusNeighborLeft = (_reset.IsEnabled ? (Control)_reset : _undo.IsEnabled ? _undo : first).GetPath();
	}

	public Task<CardModel[]> CardsOrdered() => _completion.Task;

	private void OnPeekToggled(NPeekButton button)
	{
		MouseFilter = button.IsPeeking ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
		NHoverTipSet.Clear();
		if (!button.IsPeeking)
		{
			ActiveScreenContext.Instance.Update();
			UpdateStatus();
		}
	}

	private void OnConfirm(NButton button)
	{
		if (!CanInteract || !_grid.CanEdit)
			return;
		_closed = true;
		_completion.TrySetResult(_grid.OrderedCards.ToArray());
		NOverlayStack.Instance?.Remove(this);
	}

	public override void _ExitTree()
	{
		_closed = true;
		_completion.TrySetResult(_original);
		ActiveScreenContext.Instance.Updated -= OnContextUpdated;
		if (_grid != null) _grid.OrderChanged -= UpdateStatus;
		if (_undo != null) _undo.Disconnect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUndo));
		if (_reset != null) _reset.Disconnect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnReset));
		if (_confirm != null)
			_confirm.Disconnect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnConfirm));
		if (_peekButton != null)
		{
			_peekButton.SetPeeking(false);
			_peekButton.Disconnect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(OnPeekToggled));
		}
	}

	public void AfterOverlayOpened() { }

	public void AfterOverlayClosed()
	{
		_closed = true;
		_completion.TrySetResult(_original);
		_peekButton.SetPeeking(false);
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		Show();
		_confirm.Enable();
		if (CombatManager.Instance.IsInProgress)
			_peekButton.Enable();
		UpdateStatus();
	}

	public void AfterOverlayHidden()
	{
		Hide();
		_confirm.Disable();
		_peekButton.Disable();
		_undo.Disable();
		_reset.Disable();
	}
}
