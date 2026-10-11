using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Screens;

public partial class NNagatoFleetVoteScreen : Control, IOverlayScreen
{
	public const string ScenePath = "res://NagatoSpire2/scenes/screens/fleet_vote_screen.tscn";
	public static IEnumerable<string> AssetPaths =>
	[
		ScenePath, NNagatoFleetVoteCardHolder.ScenePath, NNagatoFleetVoteCardHolder.CardScenePath,
		NagatoChoiceTable.ScenePath, NagatoChoiceTable.BackgroundPath,
		"res://scenes/ui/hand_image.tscn", .. NagatoChoicePointingImage.AssetPaths,
		.. NMultiplayerVoteContainer.AssetPaths
	];

	private IReadOnlyList<CardModel> _options = [];
	private IReadOnlyList<Player> _players = [];
	private Player _local = null!;
	private IReadOnlyDictionary<ulong, int> _votes = null!;
	private Action<int> _submitVote = null!;
	private readonly CancellationTokenSource _lifetime = new();
	private readonly List<NNagatoFleetVoteCardHolder> _holders = [];
	private readonly List<NMultiplayerVoteContainer> _voteContainers = [];
	private readonly List<NHandImage> _hands = [];
	private NagatoChoiceTable _table = null!;
	private Control _row = null!;
	private NPeekButton _peek = null!;
	private bool _ownsCursor;
	private Control? _remoteCursors;
	private bool _remoteCursorsVisible;
	private ulong _openedTicks;
	private bool _resolving;
	private int _highlightedOption = -1;

	public CancellationToken Lifetime => _lifetime.Token;
	public NetScreenType ScreenType => NetScreenType.CardSelection;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => _peek.IsPeeking ? NCombatRoom.Instance?.DefaultFocusedControl : _holders.FirstOrDefault();

	public static NNagatoFleetVoteScreen Show(IReadOnlyList<CardModel> options, IReadOnlyList<Player> players,
		Player local, IReadOnlyDictionary<ulong, int> votes, Action<int> submitVote)
	{
		var screen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NNagatoFleetVoteScreen>();
		screen._options = options;
		screen._players = players;
		screen._local = local;
		screen._votes = votes;
		screen._submitVote = submitVote;
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
			var holder = NNagatoFleetVoteCardHolder.Create(_options[i]);
			_row.AddChild(holder);
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
			NagatoChoicePointingImage.Configure(hand);
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
		if (_resolving || _lifetime.IsCancellationRequested || Time.GetTicksMsec() - _openedTicks < 350 || _peek.IsPeeking ||
			!ReferenceEquals(ActiveScreenContext.Instance.GetCurrentScreen(), this))
			return;
		_submitVote(index);
	}

	public void RefreshVotes()
	{
		if (_resolving)
			return;
		_table.GetNode<Label>("Design/Instruction").Text = new LocString("gameplay_ui",
			_votes.ContainsKey(_local.NetId) ? "NAGATO_FLEET_VOTE.waiting" : "NAGATO_FLEET_VOTE.instruction").GetFormattedText();
		foreach (var container in _voteContainers)
		{
			container.RefreshPlayerVotes(false);
			container.Scale = Vector2.One * 1.25f;
			var icons = container.GetChildren().OfType<TextureRect>().Where(icon => !icon.IsQueuedForDeletion()).ToArray();
			for (int i = 0; i < icons.Length; i++)
			{
				icons[i].ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				icons[i].Size = new Vector2(48f, 48f);
				icons[i].PivotOffset = icons[i].Size * 0.5f;
				icons[i].Position = new Vector2((i - (icons.Length - 1) * 0.5f) * 52f - 24f, 0f);
			}
		}
	}

	public async Task AnimateResult(IReadOnlyList<int> votes, IReadOnlyList<int> candidates, int winner, ulong seed)
	{
		_votes = _players.Select((player, index) => (player.NetId, Vote: votes[index]))
			.ToDictionary(entry => entry.NetId, entry => entry.Vote);
		RefreshVotes();
		_resolving = true;
		_peek.SetPeeking(false);
		_peek.Disable();
		foreach (var holder in _holders)
			holder.SetClickable(false);
		foreach (var hand in _hands)
			hand.Hide();
		_table.GetNode<Label>("Design/Instruction").Text = "";
		void Highlight(int index)
		{
			if (_highlightedOption == index)
				return;
			if (_highlightedOption >= 0)
				foreach (var player in _voteContainers[_highlightedOption].Players)
					_voteContainers[_highlightedOption].SetPlayerHighlighted(player, false);
			_highlightedOption = index;
			foreach (var player in _voteContainers[index].Players)
				_voteContainers[index].SetPlayerHighlighted(player, true);
			_table.SetFocusedOption(index);
		}
		if (candidates.Count > 1)
		{
			var rng = new Rng(seed);
			int ticks = rng.NextInt(12, 18);
			int stop = candidates.ToList().IndexOf(winner);
			var tween = CreateTween();
			tween.TweenMethod(Callable.From<float>(value =>
			{
				int remaining = ticks - Mathf.RoundToInt(value * ticks);
				int index = candidates[((stop - remaining) % candidates.Count + candidates.Count) % candidates.Count];
				if (_highlightedOption == index)
					return;
				Highlight(index);
				NDebugAudioManager.Instance?.Play("map_split_tick.mp3", 0.15f, PitchVariance.Small);
			}), 0f, 1f, 1.2).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.TweenInterval(rng.NextFloat(0.05f, 0.3f));
			if (!await tween.AwaitFinished(this))
				return;
		}
		Highlight(winner);
		_voteContainers[winner].BouncePlayers();
		var hold = CreateTween();
		hold.TweenInterval(0.5);
		await hold.AwaitFinished(this);
	}

	private void Layout()
	{
		_row.Scale = Vector2.One * _table.LayoutScale;
		_row.GlobalPosition = GlobalPosition + Size * 0.5f;
		for (int i = 0; i < _holders.Count; i++)
		{
			_holders[i].GlobalPosition = _table.OptionGlobalPosition(i);
			_voteContainers[i].Position = _holders[i].Position + new Vector2(0f, 205f);
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
		if (!_resolving)
			_table.SetFocusedOption(active ? focused : -1);
		foreach (var hand in _hands)
		{
			hand.Visible = active && !_resolving;
			if (!hand.Visible)
				continue;
			Vector2 point = LocalContext.IsMe(hand.Player) ? GetGlobalMousePosition()
				: RunManager.Instance.InputSynchronizer.GetControlSpaceFocusPosition(hand.Player.NetId, this);
			if (LocalContext.IsMe(hand.Player) && controller && focused >= 0)
				point = _table.OptionGlobalPosition(focused) + new Vector2(25f, 110f) * _table.LayoutScale;
			hand.SetPointingPosition(point);
			hand.SetIsDown(RunManager.Instance.InputSynchronizer.GetMouseDown(hand.Player.NetId));
		}
		SetCursorHidden(active && GetWindow().HasFocus());
		SetRemoteCursorsHidden(active);
	}

	private void SetCursorHidden(bool hidden)
	{
		if (hidden && !_ownsCursor)
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

	private void SetRemoteCursorsHidden(bool hidden)
	{
		if (hidden)
		{
			if (_remoteCursors == null && NGame.Instance?.RemoteCursorContainer is { } cursors)
			{
				_remoteCursors = cursors;
				_remoteCursorsVisible = cursors.Visible;
			}
			_remoteCursors?.Hide();
		}
		else if (_remoteCursors != null)
		{
			if (GodotObject.IsInstanceValid(_remoteCursors))
				_remoteCursors.Visible = _remoteCursorsVisible;
			_remoteCursors = null;
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
		SetRemoteCursorsHidden(false);
		QueueFree();
	}
	public void Close()
	{
		if (GodotObject.IsInstanceValid(this) && IsInsideTree() && !IsQueuedForDeletion())
			NOverlayStack.Instance?.Remove(this);
	}
	public void AfterOverlayShown() { Show(); if (!_resolving) _peek.Enable(); }
	public void AfterOverlayHidden() { Hide(); _peek.Disable(); SetCursorHidden(false); SetRemoteCursorsHidden(false); }
	public override void _ExitTree()
	{
		Resized -= Layout;
		SetCursorHidden(false);
		SetRemoteCursorsHidden(false);
		_lifetime.Cancel();
	}
}
