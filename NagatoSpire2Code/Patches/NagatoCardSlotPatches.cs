using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoCardSlotPatches
{
	public sealed class PlayPatch : IPatchMethod
	{
		public static string PatchId => "nagato_card_slots_play";
		public static string Description => "Track Salvo plays and load a played card only after its native resolution completes";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(CardModel), nameof(CardModel.OnPlayWrapper),
				[typeof(PlayerChoiceContext), typeof(Creature), typeof(bool), typeof(ResourceInfo), typeof(bool)])
		];

		[HarmonyPrefix]
		internal static void Prefix(CardModel __instance, out NagatoCardSlots.PlayEntry? __state) =>
			__state = NagatoCardSlots.BeginPlay(__instance);

		[HarmonyPostfix]
		internal static void Postfix(PlayerChoiceContext choiceContext, ref Task __result, NagatoCardSlots.PlayEntry? __state)
		{
			if (__state != null)
				__result = Complete(__result, choiceContext, __state);
		}

		private static async Task Complete(Task original, PlayerChoiceContext context, NagatoCardSlots.PlayEntry entry)
		{
			try { await original; }
			catch
			{
				NagatoCardSlots.EndPlay(entry);
				throw;
			}
			await NagatoCardSlots.FinishPlay(context, entry);
		}
	}

	public sealed class DrawPatch : IPatchMethod
	{
		public static string PatchId => "nagato_load_after_draw";
		public static string Description => "Load sealed cards in draw order after native draw hooks finish";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(Hook), nameof(Hook.AfterCardDrawn),
				[typeof(ICombatState), typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)])
		];

		[HarmonyPrefix]
		public static void Prefix(CardModel card, out NagatoCardSlots.DrawEntry? __state) => __state = NagatoCardSlots.BeginDraw(card);

		[HarmonyPostfix]
		public static void Postfix(PlayerChoiceContext choiceContext, ref Task __result, NagatoCardSlots.DrawEntry? __state)
		{
			if (__state != null)
				__result = Complete(__result, choiceContext, __state);
		}

		private static async Task Complete(Task original, PlayerChoiceContext context, NagatoCardSlots.DrawEntry entry)
		{
			try
			{
				await original;
				await NagatoCardSlots.FinishDraw(context, entry);
			}
			catch
			{
				NagatoCardSlots.CancelDraw(entry);
				throw;
			}
		}
	}

	public sealed class ReadyPatch : IPatchMethod
	{
		public static string PatchId => "nagato_card_slots_ready";
		public static string Description => "Attach the loading-slot display to each player's orb manager";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() => [new(typeof(NOrbManager), nameof(NOrbManager._Ready), [])];
		[HarmonyPostfix]
		public static void Postfix(NOrbManager __instance) => NNagatoCardSlots.Attach(__instance);
	}

	public sealed class LayoutPatch : IPatchMethod
	{
		public static string PatchId => "nagato_card_slots_orb_layout";
		public static string Description => "Lay out native orb slots horizontally above active loading slots";
		public static bool IsCritical => true;
		public static ModPatchTarget[] GetTargets() => [new(typeof(NOrbManager), "TweenLayout", [])];
		[HarmonyPrefix]
		public static bool Prefix(NOrbManager __instance) => !NNagatoCardSlots.TryLayout(__instance);
	}
}
