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
		_grid = GetNode<NNagatoCardOrderGrid>("CardOrderGrid");
		_confirm = GetNode<NConfirmButton>("Confirm");
		_peekButton = GetNode<NPeekButton>("PeekButton");
		GetNode<Label>("Title").Text = new LocString("cards", "NAGATO_SPIRE2_CARD_BATTLE_DISPOSITION.title").GetFormattedText();
		GetNode<Label>("Hint").Text = _prompt.GetFormattedText();
		_confirm.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnConfirm));
		_peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(OnPeekToggled));
		_peekButton.AddTargets(_grid, _confirm, GetNode<Control>("Background"), GetNode<Control>("Title"), GetNode<Control>("Hint"));
		_grid.CanInteract = () => CanInteract;
		_grid.NavigationFooter = _confirm;
		_grid.NavigationSidebar = _peekButton;
		_grid.SetCards(_original);
	}

	public Task<CardModel[]> CardsOrdered() => _completion.Task;

	private void OnPeekToggled(NPeekButton button)
	{
		MouseFilter = button.IsPeeking ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
		NHoverTipSet.Clear();
		if (!button.IsPeeking)
			ActiveScreenContext.Instance.Update();
	}

	private void OnConfirm(NButton button)
	{
		if (!CanInteract)
			return;
		_closed = true;
		_completion.TrySetResult(_grid.OrderedCards.ToArray());
		NOverlayStack.Instance?.Remove(this);
	}

	public override void _ExitTree()
	{
		_closed = true;
		_completion.TrySetResult(_original);
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
	}

	public void AfterOverlayHidden()
	{
		Hide();
		_confirm.Disable();
		_peekButton.Disable();
	}
}
