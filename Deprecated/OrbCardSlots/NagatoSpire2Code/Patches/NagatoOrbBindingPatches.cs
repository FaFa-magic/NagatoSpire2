using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Combat;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoOrbBindingPatches
{
	private static readonly AccessTools.FieldRef<OrbQueue, Player> QueueOwner =
		AccessTools.FieldRefAccess<OrbQueue, Player>("_owner");

	public sealed class PileAddPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_pile_add";
		public static string Description => "Resolve orb bindings after a complete card-pile add";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(CardPileCmd), nameof(CardPileCmd.Add),
				[typeof(IEnumerable<CardModel>), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool), typeof(bool)])
		];

		[HarmonyPrefix]
		public static void Prefix(IEnumerable<CardModel> cards, CardPile newPile, out (Player?, PlayerCombatState?) __state)
		{
			var player = newPile.IsCombatPile ? cards.FirstOrDefault()?.Owner : null;
			__state = (player, player == null ? null : NagatoOrbBindings.BeginPileChange(player));
		}

		[HarmonyPostfix]
		public static void Postfix((Player? Player, PlayerCombatState? Combat) __state, ref Task<IReadOnlyList<CardPileAddResult>> __result)
		{
			if (__state.Player != null && __state.Combat != null)
				__result = Finish(__result, __state.Player, __state.Combat);
		}

		private static async Task<IReadOnlyList<CardPileAddResult>> Finish(Task<IReadOnlyList<CardPileAddResult>> original, Player player, PlayerCombatState combat)
		{
			try { return await original; }
			finally { await NagatoOrbBindings.EndPileChange(player, combat); }
		}
	}

	public sealed class ShufflePatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_shuffle";
		public static string Description => "Bind only after the shuffle and its hooks finish";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(CardPileCmd), nameof(CardPileCmd.Shuffle), [typeof(PlayerChoiceContext), typeof(Player)])
		];

		[HarmonyPrefix]
		public static void Prefix(Player player, out PlayerCombatState? __state) => __state = NagatoOrbBindings.BeginPileChange(player);

		[HarmonyPostfix]
		public static void Postfix(PlayerChoiceContext choiceContext, Player player, PlayerCombatState? __state, ref Task __result)
		{
			if (__state != null)
				__result = Finish(__result, choiceContext, player, __state);
		}

		private static async Task Finish(Task original, PlayerChoiceContext context, Player player, PlayerCombatState combat)
		{
			try { await original; }
			finally { await NagatoOrbBindings.EndPileChange(player, combat, context); }
		}
	}

	public sealed class CardMovedPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_card_moved";
		public static string Description => "Carry a mapped orb into the hand before card movement hooks";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(Hook), nameof(Hook.AfterCardChangedPiles),
				[typeof(IRunState), typeof(ICombatState), typeof(CardModel), typeof(PileType), typeof(AbstractModel)])
		];

		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		public static void Prefix(CardModel card, PileType oldPile) => NagatoOrbBindings.CardMoved(card, oldPile);
	}

	public sealed class CapacityPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_capacity";
		public static string Description => "Keep orb-card positions in sync with native capacity changes";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(OrbQueue), nameof(OrbQueue.AddCapacity), [typeof(int)]),
			new(typeof(OrbQueue), nameof(OrbQueue.RemoveCapacity), [typeof(int)]),
			new(typeof(OrbQueue), nameof(OrbQueue.Clear), [])
		];

		[HarmonyPostfix]
		public static void Postfix(OrbQueue __instance) => NagatoOrbBindings.QueueChanged(QueueOwner(__instance));
	}

	public sealed class AddSlotsPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_add_slots";
		public static string Description => "Return carried orbs when new card slots become available";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() => [new(typeof(OrbCmd), nameof(OrbCmd.AddSlots), [typeof(Player), typeof(int)])];

		[HarmonyPostfix]
		public static void Postfix(Player player, ref Task __result) => __result = Finish(__result, player);

		private static async Task Finish(Task original, Player player)
		{
			await original;
			await NagatoOrbBindings.Reconcile(new BlockingPlayerChoiceContext(), player);
		}
	}

	public sealed class TurnEndPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_binding_turn_end";
		public static string Description => "Trigger native passives of orbs carried by cards still in hand";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(OrbQueue), nameof(OrbQueue.BeforeTurnEnd), [typeof(PlayerChoiceContext)])
		];

		[HarmonyPrefix]
		public static void Prefix(OrbQueue __instance, out (CardModel Card, OrbModel Orb)[] __state) =>
			__state = NagatoOrbBindings.GetHandOrbs(QueueOwner(__instance));

		[HarmonyPostfix]
		public static void Postfix(OrbQueue __instance, PlayerChoiceContext choiceContext,
			(CardModel Card, OrbModel Orb)[] __state, ref Task __result)
		{
			if (__state.Length > 0)
				__result = Finish(__result, choiceContext, QueueOwner(__instance), __state);
		}

		private static async Task Finish(Task original, PlayerChoiceContext context, Player player,
			(CardModel Card, OrbModel Orb)[] snapshot)
		{
			await original;
			await NagatoOrbBindings.TriggerHandPassives(context, player, snapshot);
		}
	}
}
