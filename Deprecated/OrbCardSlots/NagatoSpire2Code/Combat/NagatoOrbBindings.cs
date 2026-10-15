using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Commands;
using STS2RitsuLib.Utils;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoOrbBindings
{
	private sealed class State
	{
		public OrbModel?[] Slots = [];
		public readonly Dictionary<CardModel, OrbModel> Mapped = [];
		public readonly Dictionary<CardModel, OrbModel> Attached = [];
		public int PileChanges;
		public bool Resolving;
		public bool Dirty;
	}

	private static readonly AttachedState<PlayerCombatState, State> States = new(() => new State());
	private static readonly AccessTools.FieldRef<OrbQueue, List<OrbModel>> QueueOrbs =
		AccessTools.FieldRefAccess<OrbQueue, List<OrbModel>>("_orbs");

	private static bool IsActive(Player player) => NagatoOrbSlotRun.AppliesTo(player) &&
		player.PlayerCombatState != null && player.Creature.IsAlive && !CombatManager.Instance.IsOverOrEnding;

	public static OrbModel?[] GetSlots(Player player)
	{
		if (player.PlayerCombatState is not { } combat || !NagatoOrbSlotRun.AppliesTo(player))
			return [];
		if (States.TryGetValue(combat, out var state))
			return state.Slots.ToArray();
		var slots = new OrbModel?[combat.OrbQueue.Capacity];
		for (int i = 0; i < Math.Min(slots.Length, combat.OrbQueue.Orbs.Count); i++)
			slots[i] = combat.OrbQueue.Orbs[i];
		return slots;
	}

	public static OrbModel? GetAttachedOrb(CardModel card) => card.IsMutable && card.Owner?.PlayerCombatState is { } combat &&
		NagatoOrbSlotRun.AppliesTo(card.Owner) && States.TryGetValue(combat, out var state)
			? state.Attached.GetValueOrDefault(card) : null;

	public static bool IsMapped(CardModel card, OrbModel orb) => card.IsMutable && card.Pile?.Type == PileType.Draw &&
		card.Owner?.PlayerCombatState is { } combat && NagatoOrbSlotRun.AppliesTo(card.Owner) &&
		States.TryGetValue(combat, out var state) && state.Mapped.TryGetValue(card, out var mapped) && mapped == orb &&
		combat.OrbQueue.Orbs.Contains(orb);

	public static IEnumerable<OrbModel> GetAttachedOrbs(Player player)
	{
		if (!NagatoOrbSlotRun.AppliesTo(player) || player.PlayerCombatState is not { } combat ||
			!States.TryGetValue(combat, out var state))
			return [];
		return combat.AllPiles.SelectMany(pile => pile.Cards).Select(card => state.Attached.GetValueOrDefault(card))
			.OfType<OrbModel>().Distinct().ToArray();
	}

	public static (CardModel Card, OrbModel Orb)[] GetHandOrbs(Player player) =>
		player.PlayerCombatState?.Hand.Cards.Select(card => (Card: card, Orb: GetAttachedOrb(card)))
			.Where(pair => pair.Orb != null).Select(pair => (pair.Card, pair.Orb!)).ToArray() ?? [];

	public static void Initialize(Player player)
	{
		if (!IsActive(player))
			return;
		var state = States.GetOrCreate(player.PlayerCombatState!);
		SyncQueue(player, state);
		UpdateMapping(player, state);
	}

	public static PlayerCombatState? BeginPileChange(Player player)
	{
		if (!IsActive(player))
			return null;
		var combat = player.PlayerCombatState!;
		if (!States.ContainsKey(combat))
			Initialize(player);
		var state = States.GetOrCreate(combat);
		if (state.PileChanges == 0 && !state.Resolving)
			UpdateMapping(player, state);
		state.PileChanges++;
		return combat;
	}

	public static async Task EndPileChange(Player player, PlayerCombatState? combat, PlayerChoiceContext? choiceContext = null)
	{
		if (combat == null || !States.TryGetValue(combat, out var state))
			return;
		state.PileChanges--;
		if (state.PileChanges == 0 && player.PlayerCombatState == combat)
			await Reconcile(choiceContext ?? new BlockingPlayerChoiceContext(), player);
	}

	public static void CardMoved(CardModel card, PileType oldPile)
	{
		var player = card.Owner;
		if (oldPile != PileType.Draw || card.Pile?.Type != PileType.Hand || !IsActive(player) ||
			!States.TryGetValue(player.PlayerCombatState!, out var state) || state.Attached.ContainsKey(card))
			return;
		if (!state.Mapped.Remove(card, out var orb) ||
			!player.PlayerCombatState!.OrbQueue.Orbs.Contains(orb))
			return;
		int index = Array.IndexOf(state.Slots, orb);
		if (index < 0)
			return;
		Array.Copy(state.Slots, index + 1, state.Slots, index, state.Slots.Length - index - 1);
		state.Slots[^1] = null;
		player.PlayerCombatState.OrbQueue.Remove(orb);
		state.Attached.Add(card, orb);
	}

	public static void QueueChanged(Player player)
	{
		if (!IsActive(player))
			return;
		var state = States.GetOrCreate(player.PlayerCombatState!);
		SyncQueue(player, state);
	}

	private static void SyncQueue(Player player, State state)
	{
		var queue = player.PlayerCombatState!.OrbQueue;
		var orbs = QueueOrbs(queue);
		if (state.Slots.Length > queue.Capacity)
			foreach (var orb in state.Slots.Skip(queue.Capacity).OfType<OrbModel>())
			{
				orbs.Remove(orb);
				orb.RemoveInternal();
			}
		Array.Resize(ref state.Slots, queue.Capacity);
		for (int i = 0; i < state.Slots.Length; i++)
			if (state.Slots[i] is { } orb && !orbs.Contains(orb))
			{
				Array.Copy(state.Slots, i + 1, state.Slots, i, state.Slots.Length - i - 1);
				state.Slots[^1] = null;
				i--;
			}
		foreach (var orb in orbs.ToArray())
		{
			if (state.Slots.Contains(orb))
				continue;
			int index = Array.IndexOf(state.Slots, null);
			if (index >= 0)
				state.Slots[index] = orb;
		}
		SortQueue(queue, state);
	}

	private static void SortQueue(OrbQueue queue, State state)
	{
		var orbs = QueueOrbs(queue);
		orbs.Clear();
		orbs.AddRange(state.Slots.OfType<OrbModel>());
	}

	private static void UpdateMapping(Player player, State state)
	{
		var cards = NagatoOrbCardSlots.GetCards(player);
		state.Mapped.Clear();
		for (int i = 0; i < cards.Length; i++)
			if (cards[i] is { } card && state.Slots.ElementAtOrDefault(i) is { } orb)
				state.Mapped[card] = orb;
	}

	public static async Task Reconcile(PlayerChoiceContext choiceContext, Player player)
	{
		if (!IsActive(player))
			return;
		var combat = player.PlayerCombatState!;
		var state = States.GetOrCreate(combat);
		if (state.PileChanges > 0 || state.Resolving)
		{
			state.Dirty = true;
			return;
		}
		state.Resolving = true;
		try
		{
			do
			{
				state.Dirty = false;
				SyncQueue(player, state);
				var cards = NagatoOrbCardSlots.GetCards(player);
				UpdateMapping(player, state);
				for (int i = 0; i < cards.Length; i++)
				{
					if (!IsActive(player) || player.PlayerCombatState != combat)
						return;
					if (cards[i] is not { } card || !state.Attached.Remove(card, out var carried))
						continue;
					if (state.Slots[i] == null)
					{
						state.Slots[i] = carried;
						combat.OrbQueue.Insert(combat.OrbQueue.Orbs.Count, carried);
						SortQueue(combat.OrbQueue, state);
						continue;
					}
					int empty = Array.IndexOf(state.Slots, null);
					if (empty >= 0)
					{
						state.Slots[empty] = carried;
						combat.OrbQueue.Insert(combat.OrbQueue.Orbs.Count, carried);
						SortQueue(combat.OrbQueue, state);
					}
					else
					{
						await NagatoOrbCmd.EvokeDetached(choiceContext, player, carried);
					}
					state.Dirty = true;
					break;
				}
				UpdateMapping(player, state);
			} while (state.Dirty && IsActive(player) && player.PlayerCombatState == combat);
		}
		finally
		{
			state.Resolving = false;
		}
	}

	public static async Task TriggerHandPassives(PlayerChoiceContext choiceContext, Player player,
		IEnumerable<(CardModel Card, OrbModel Orb)> snapshot)
	{
		foreach (var (card, orb) in snapshot)
		{
			if (!IsActive(player))
				break;
			if (card.Pile?.Type == PileType.Hand && card.Owner == player && GetAttachedOrb(card) == orb)
				await OrbCmd.Passive(choiceContext, orb, null, countAffectedByHooks: true);
		}
	}

	public static void Clear(Player player)
	{
		if (player.PlayerCombatState is { } combat && States.TryGetValue(combat, out var state))
		{
			foreach (var orb in state.Attached.Values)
				orb.RemoveInternal();
			States.Remove(combat);
		}
	}
}
