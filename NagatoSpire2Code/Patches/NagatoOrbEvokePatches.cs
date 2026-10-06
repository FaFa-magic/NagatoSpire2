using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Relics;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoOrbResolutionScope
{
	private sealed class State
	{
		public int Depth;
		public bool RefillPending;
	}

	private static readonly Dictionary<Player, State> States = [];
	private static readonly object Gate = new();

	public static void Enter(Player player)
	{
		lock (Gate)
		{
			if (!States.TryGetValue(player, out State? state))
			{
				state = new State();
				States.Add(player, state);
			}
			state.Depth++;
		}
	}

	public static bool DeferRefill(Player player)
	{
		lock (Gate)
		{
			if (!States.TryGetValue(player, out State? state))
				return false;
			state.RefillPending = true;
			return true;
		}
	}

	public static async Task Exit(Player player, PlayerChoiceContext choiceContext)
	{
		bool refill;
		lock (Gate)
		{
			State state = States[player];
			state.Depth--;
			if (state.Depth > 0)
				return;
			refill = state.RefillPending;
			States.Remove(player);
		}
		if (refill && player.GetRelic<NagatoShellRefillRelic>() is { } relic)
			await relic.FillEmptySlots(choiceContext);
	}
}

public sealed class NagatoOrbEvokePatch : IPatchMethod
{
	public static string PatchId => "nagato_shell_orb_ordered_evoke";
	public static string Description => "Resolve neighboring Nagato shells before moving and refilling orb slots";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(OrbCmd), "Evoke",
			[typeof(PlayerChoiceContext), typeof(Player), typeof(OrbModel), typeof(bool)])
	];

	[HarmonyPrefix]
	public static bool Prefix(
		PlayerChoiceContext choiceContext,
		Player player,
		OrbModel evokedOrb,
		bool dequeue,
		ref Task __result)
	{
		if (evokedOrb is not NagatoShellOrb ||
			!player.PlayerCombatState!.OrbQueue.Orbs.Contains(evokedOrb))
			return true;

		__result = EvokeShells(choiceContext, player, evokedOrb, dequeue);
		return false;
	}

	private static async Task EvokeShells(
		PlayerChoiceContext choiceContext,
		Player player,
		OrbModel evokedOrb,
		bool dequeue)
	{
		if (CombatManager.Instance.IsOverOrEnding)
			return;

		OrbQueue queue = player.PlayerCombatState!.OrbQueue;
		OrbModel[] original = queue.Orbs.ToArray();
		int index = Array.IndexOf(original, evokedOrb);
		if (index < 0)
			return;

		List<OrbModel> chain = [evokedOrb];
		bool allAdjacent = player.GetRelic<BigShipsBigGunsDivineMight>() != null;
		bool leftActive = true;
		bool rightActive = true;
		for (int distance = 1; leftActive || rightActive; distance++)
		{
			int left = index - distance;
			int right = index + distance;
			if (leftActive && left >= 0)
			{
				if (original[left].GetType() == evokedOrb.GetType())
					chain.Add(original[left]);
				else if (!allAdjacent)
					leftActive = false;
			}
			else
				leftActive = false;
			if (rightActive && right < original.Length)
			{
				if (original[right].GetType() == evokedOrb.GetType())
					chain.Add(original[right]);
				else if (!allAdjacent)
					rightActive = false;
			}
			else
				rightActive = false;
		}

		NagatoOrbResolutionScope.Enter(player);
		try
		{
			List<(OrbModel Orb, IEnumerable<Creature> Targets)> resolved = [];
			foreach (OrbModel orb in chain)
			{
				if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
					break;
				if (!queue.Orbs.Contains(orb))
					continue;
				choiceContext.PushModel(orb);
				try
				{
					IEnumerable<Creature> targets = await orb.Evoke(choiceContext);
					resolved.Add((orb, targets.ToArray()));
				}
				finally
				{
					choiceContext.PopModel(orb);
				}
			}

			foreach ((OrbModel orb, _) in resolved)
			{
				if (orb == evokedOrb && !dequeue)
					continue;
				if (queue.Remove(orb))
					NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.EvokeOrbAnim(orb);
			}

			foreach ((OrbModel orb, IEnumerable<Creature> targets) in resolved)
			{
				if (player.Creature.CombatState is not { } combatState)
					break;
				await Hook.AfterOrbEvoked(choiceContext, combatState, orb, targets);
				if (orb != evokedOrb || dequeue)
					orb.RemoveInternal();
			}
		}
		finally
		{
			await NagatoOrbResolutionScope.Exit(player, choiceContext);
		}
	}
}

public sealed class NagatoOrbChannelPatch : IPatchMethod
{
	public static string PatchId => "nagato_shell_orb_channel_refill_order";
	public static string Description => "Refill Nagato orb slots after overflow channeling completes";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(OrbCmd), nameof(OrbCmd.Channel),
			[typeof(PlayerChoiceContext), typeof(OrbModel), typeof(Player)])
	];

	[HarmonyPrefix]
	public static void Prefix(Player player, out bool __state)
	{
		__state = player.GetRelic<NagatoShellRefillRelic>() is not null;
		if (__state)
			NagatoOrbResolutionScope.Enter(player);
	}

	[HarmonyPostfix]
	public static void Postfix(
		PlayerChoiceContext choiceContext,
		Player player,
		bool __state,
		ref Task __result)
	{
		if (__state)
			__result = FinishChannel(__result, player, choiceContext);
	}

	private static async Task FinishChannel(
		Task original,
		Player player,
		PlayerChoiceContext choiceContext)
	{
		try
		{
			await original;
		}
		finally
		{
			await NagatoOrbResolutionScope.Exit(player, choiceContext);
		}
	}
}
