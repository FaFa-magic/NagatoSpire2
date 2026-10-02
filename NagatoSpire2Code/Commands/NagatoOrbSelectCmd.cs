using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoOrbSelectCmd
{
	public static async Task<int?> Select(PlayerChoiceContext context, Player player, OrbModel[] choices)
	{
		if (choices.Length == 0)
			return null;

		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
		await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
		try
		{
			if (LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				NPlayerHand.Instance?.CancelAllCardPlay();
				int? index = await SelectLocal(player, choices);
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(index));
				return index;
			}

			int remoteIndex = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndex();
			return remoteIndex >= 0 && remoteIndex < choices.Length ? remoteIndex : null;
		}
		finally
		{
			await context.SignalPlayerChoiceEnded();
		}
	}

	private static async Task<int?> SelectLocal(Player player, OrbModel[] choices)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		NOrbManager? manager = room?.GetCreatureNode(player.Creature)?.OrbManager;
		if (room == null || manager == null)
			return null;

		var selection = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
		var active = choices.ToHashSet();
		NOrb[] orbs = manager.GetNode<Control>("%Orbs").GetChildren()
			.OfType<NOrb>()
			.Where(orb => orb.Model != null && active.Contains(orb.Model))
			.ToArray();
		if (orbs.Length == 0)
			return null;

		var panel = new PanelContainer
		{
			Name = "NagatoOrbSelection",
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			OffsetLeft = -260f,
			OffsetRight = 260f,
			OffsetTop = 36f,
			MouseFilter = Control.MouseFilterEnum.Stop,
			ZIndex = 100
		};
		var row = new HBoxContainer();
		var label = new Label
		{
			Text = new LocString("cards", "NAGATO_SPIRE2_CARD_TARGETED_RELOAD.selection").GetFormattedText(),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		var skip = new Button
		{
			Text = new LocString("cards", "NAGATO_SPIRE2_CARD_TARGETED_RELOAD.skip").GetFormattedText()
		};
		row.AddChild(label);
		row.AddChild(skip);
		panel.AddChild(row);
		room.Ui.AddChild(panel);

		var handlers = new List<(NOrb Orb, NClickableControl.ReleasedEventHandler Handler)>();
		foreach (NOrb orb in orbs)
		{
			int index = Array.IndexOf(choices, orb.Model);
			if (index < 0)
				continue;
			NClickableControl.ReleasedEventHandler handler = _ => selection.TrySetResult(index);
			orb.Released += handler;
			handlers.Add((orb, handler));
		}

		void ChooseLast() => selection.TrySetResult(null);
		skip.Pressed += ChooseLast;
		room.TreeExiting += ChooseLast;
		NHotkeyManager.Instance?.PushHotkeyPressedBinding(MegaInput.cancel, ChooseLast);
		orbs[0].GrabFocus();
		try
		{
			return await selection.Task;
		}
		finally
		{
			foreach ((NOrb orb, NClickableControl.ReleasedEventHandler handler) in handlers)
			{
				if (GodotObject.IsInstanceValid(orb))
					orb.Released -= handler;
			}
			skip.Pressed -= ChooseLast;
			if (GodotObject.IsInstanceValid(room))
				room.TreeExiting -= ChooseLast;
			NHotkeyManager.Instance?.RemoveHotkeyPressedBinding(MegaInput.cancel, ChooseLast);
			if (GodotObject.IsInstanceValid(panel))
				panel.QueueFree();
		}
	}

	public static void SyncVisualOrder(Player player)
	{
		NOrbManager? manager = NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager;
		if (manager == null || player.PlayerCombatState == null)
			return;

		var queue = player.PlayerCombatState.OrbQueue;
		var active = queue.Orbs.ToHashSet();
		NOrb[] slots = manager.GetNode<Control>("%Orbs").GetChildren()
			.OfType<NOrb>()
			.Where(orb => orb.Model == null || active.Contains(orb.Model))
			.Take(queue.Capacity)
			.ToArray();
		if (slots.Length != queue.Capacity)
			return;

		for (int i = 0; i < queue.Orbs.Count; i++)
		{
			if (slots[i].Model != queue.Orbs[i])
				slots[i].ReplaceOrb(queue.Orbs[i]);
		}
	}
}
