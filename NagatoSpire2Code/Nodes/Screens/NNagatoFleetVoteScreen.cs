using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Screens;

public partial class NNagatoFleetVoteScreen : Control, IOverlayScreen
{
	public const string ScenePath = "res://NagatoSpire2/scenes/screens/fleet_vote_screen.tscn";
	public static IEnumerable<string> AssetPaths =>
	[
		ScenePath, NagatoChoiceTable.ScenePath, NagatoChoiceTable.BackgroundPath,
		"res://scenes/ui/hand_image.tscn", .. NMultiplayerVoteContainer.AssetPaths
	];

	private IReadOnlyList<CardModel> _options = [];
	private IReadOnlyList<Player> _players = [];
	private Player _local = null!;
	private IReadOnlyDictionary<ulong, int> _votes = null!;
	private readonly TaskCompletionSource<int> _submitted = new();
	private readonly List<NGridCardHolder> _holders = [];
	private readonly List<NMultiplayerVoteContainer> _voteContainers = [];
	private readonly List<NHandImage> _hands = [];
	private NagatoChoiceTable _table = null!;
	private Control _row = null!;
	private NPeekButton _peek = null!;
	private bool _ownsCursor;
	private ulong _openedTicks;

	public Task<int> VoteSubmitted => _submitted.Task;
	public NetScreenType ScreenType => NetScreenType.CardSelection;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => _peek.IsPeeking ? NCombatRoom.Instance?.DefaultFocusedControl : _holders.FirstOrDefault();

	public static NNagatoFleetVoteScreen Show(IReadOnlyList<CardModel> options, IReadOnlyList<Player> players,
		Player local, IReadOnlyDictionary<ulong, int> votes)
	{
		var screen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NNagatoFleetVoteScreen>();
		screen._options = options;
		screen._players = players;
		screen._local = local;
		screen._votes = votes;
		NPlayerHand.Instance?.CancelAllCardPlay();
		(NOverlayStack.Instance ?? throw new InvalidOperationException("Fleet voting requires an overlay stack.")).Push(screen);
		return screen;
	}

	public override void _Ready()
	{
		GetNode<Control>("Banner").Hide();
		GetNode<Control>("SkipButton").Hide();
		var piles = GetNode<NCombatPilesContainer>("%CombatPiles");
		piles.Initialize(_local);
		piles.Disable();
		piles.Hide();
		GetNode<Control>("%InspectPrompt").Hide();
		_row = GetNode<Control>("CardRow");
		_table = ResourceLoader.Load<PackedScene>(NagatoChoiceTable.ScenePath).Instantiate<NagatoChoiceTable>();
		AddChild(_table);
		MoveChild(_table, 0);
		_table.Configure(_options.Count, new LocString("gameplay_ui", "NAGATO_FLEET_VOTE.title").GetFormattedText(),
			new LocString("gameplay_ui", "NAGATO_FLEET_VOTE.instruction").GetFormattedText(), false);
		for (int i = 0; i < _options.Count; i++)
		{
			var card = NCard.Create(_options[i]) ?? throw new InvalidOperationException("Could not create a fleet vote option.");
			var holder = NGridCardHolder.Create(card) ?? throw new InvalidOperationException("Could not create a fleet vote holder.");
			_row.AddChild(holder);
			card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			holder.Scale = holder.SmallScale;
			int index = i;
			holder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(_ => SubmitVote(index)));
			holder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(_ =>
				NGame.Instance?.GetInspectCardScreen().Open(_options.ToList(), index)));
			_holders.Add(holder);
			var container = new NMultiplayerVoteContainer { MouseFilter = MouseFilterEnum.Ignore };
			_row.AddChild(container);
			container.Initialize(player => _votes.TryGetValue(player.NetId, out int vote) && vote == index, _players);
			_voteContainers.Add(container);
		}
		for (int i = 0; i < _players.Count; i++)
		{
			var hand = NHandImage.Create(_players[i], i);
			hand.MouseFilter = MouseFilterEnum.Ignore;
			hand.ZIndex = 50;
			AddChild(hand);
			if (hand.GetNode<TextureRect>("TextureRect").Texture?.GetWidth() == 422)
			{
				var texture = hand.GetNode<TextureRect>("TextureRect");
				texture.Size = new Vector2(211f, 600f);
				texture.Position = new Vector2(-130.25f, -19f);
				texture.PivotOffset = new Vector2(130.25f, 19f);
			}
			hand.AnimateIn();
			_hands.Add(hand);
		}
		_peek = GetNode<NPeekButton>("%PeekButton");
		_peek.AddTargets(_table, _row);
		_peek.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(_ =>
		{
			MouseFilter = _peek.IsPeeking ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
			piles.Visible = _peek.IsPeeking;
			if (_peek.IsPeeking)
				piles.Enable();
			else
				piles.Disable();
		}));
		Resized += Layout;
		Layout();
		RefreshVotes();
		for (int i = 0; i < _holders.Count; i++)
		{
			_holders[i].FocusNeighborLeft = _holders[(i + _holders.Count - 1) % _holders.Count].GetPath();
			_holders[i].FocusNeighborRight = _holders[(i + 1) % _holders.Count].GetPath();
		}
	}

	private void SubmitVote(int index)
	{
		if (_submitted.Task.IsCompleted || Time.GetTicksMsec() - _openedTicks < 350 || _peek.IsPeeking)
			return;
		_submitted.TrySetResult(index);
		_table.GetNode<Label>("Design/Instruction").Text = new LocString("gameplay_ui", "NAGATO_FLEET_VOTE.waiting").GetFormattedText();
	}

	public void RefreshVotes()
	{
		foreach (var container in _voteContainers)
		{
			container.RefreshPlayerVotes();
			var icons = container.GetChildren().OfType<TextureRect>().ToArray();
			for (int i = 0; i < icons.Length; i++)
			{
				icons[i].Scale = Vector2.One * 0.7f;
				icons[i].Position = new Vector2((i - (icons.Length - 1) * 0.5f) * 48f - 32f, 0f);
			}
		}
	}

	private void Layout()
	{
		_row.Scale = Vector2.One * _table.LayoutScale;
		_row.GlobalPosition = GlobalPosition + Size * 0.5f;
		for (int i = 0; i < _holders.Count; i++)
		{
			_holders[i].GlobalPosition = _table.OptionGlobalPosition(i);
			_voteContainers[i].Position = _holders[i].Position + new Vector2(0f, 240f);
		}
		foreach (var hand in _hands)
			hand.Scale = Vector2.One * _table.LayoutScale;
	}

	public override void _Process(double delta)
	{
		Layout();
		bool active = IsVisibleInTree() && !_peek.IsPeeking && ReferenceEquals(ActiveScreenContext.Instance.GetCurrentScreen(), this);
		bool controller = NControllerManager.Instance?.IsUsingDirectionalNavigation == true;
		int focused = _holders.FindIndex(holder => controller ? holder.HasFocus() : holder.Hitbox.GetGlobalRect().HasPoint(GetGlobalMousePosition()));
		_table.SetFocusedOption(active ? focused : -1);
		foreach (var hand in _hands)
		{
			hand.Visible = active;
			if (!active)
				continue;
			bool voted = _votes.TryGetValue(hand.Player.NetId, out int vote);
			Vector2 point = voted ? _table.OptionGlobalPosition(vote) + new Vector2(25f, 110f) * _table.LayoutScale
				: LocalContext.IsMe(hand.Player) ? GetGlobalMousePosition()
				: RunManager.Instance.InputSynchronizer.GetControlSpaceFocusPosition(hand.Player.NetId, this);
			if (LocalContext.IsMe(hand.Player) && !voted && NControllerManager.Instance?.IsUsingDirectionalNavigation == true && focused >= 0)
				point = _table.OptionGlobalPosition(focused) + new Vector2(25f, 110f) * _table.LayoutScale;
			hand.SetPointingPosition(point);
			hand.SetIsDown(voted || RunManager.Instance.InputSynchronizer.GetMouseDown(hand.Player.NetId));
		}
		SetCursorHidden(active && GetWindow().HasFocus() && _table.ContainsPaperPoint(GetGlobalMousePosition()) &&
			NControllerManager.Instance?.IsUsingDirectionalNavigation != true);
	}

	private void SetCursorHidden(bool hidden)
	{
		if (hidden && !_ownsCursor && Input.MouseMode == Input.MouseModeEnum.Visible)
		{
			NGame.Instance?.CursorManager.SetCursorShown(false);
			_ownsCursor = true;
		}
		else if (!hidden && _ownsCursor)
		{
			NGame.Instance?.CursorManager.SetCursorShown(true);
			_ownsCursor = false;
		}
	}

	public void AfterOverlayOpened()
	{
		_openedTicks = Time.GetTicksMsec();
		Modulate = Colors.Transparent;
		CreateTween().TweenProperty(this, "modulate:a", 1f, 0.2);
	}
	public void AfterOverlayClosed()
	{
		_peek.SetPeeking(false);
		SetCursorHidden(false);
		QueueFree();
	}
	public void AfterOverlayShown() { Show(); _peek.Enable(); }
	public void AfterOverlayHidden() { Hide(); _peek.Disable(); SetCursorHidden(false); }
	public override void _ExitTree()
	{
		Resized -= Layout;
		SetCursorHidden(false);
		_submitted.TrySetCanceled();
		foreach (var holder in _holders)
			holder.QueueFreeSafely();
	}
}
