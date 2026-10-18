using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoDrawPilePreviewPatches
{
    public sealed class InitializePatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_ui";
        public static string Description => "Attach known draw-pile previews to the local combat UI";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() => [new(typeof(NCombatCardPile), nameof(NCombatCardPile.Initialize), [typeof(Player)])];

        [HarmonyPostfix]
        public static void Postfix(NCombatCardPile __instance, Player player)
        {
            if (__instance is NDrawPileButton button && ReferenceEquals(NCombatRoom.Instance?.Ui?.DrawPile, button))
                NNagatoDrawPilePreviews.Attach(button, player);
        }
    }

    public sealed class AddPatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_add";
        public static string Description => "Record deterministic top cards after native card movement finishes";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(CardPileCmd), nameof(CardPileCmd.Add),
                [typeof(IEnumerable<CardModel>), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool), typeof(bool)])
        ];

        [HarmonyPrefix]
        public static void Prefix(CardPile newPile, CardPilePosition position, out NNagatoDrawPilePreviews.Addition? __state) =>
            __state = NNagatoDrawPilePreviews.BeginAddition(newPile, position);

        [HarmonyPostfix]
        public static void Postfix(ref Task<IReadOnlyList<CardPileAddResult>> __result, NNagatoDrawPilePreviews.Addition? __state)
        {
            if (__state is { } addition)
                __result = Complete(__result, addition);
        }

        private static async Task<IReadOnlyList<CardPileAddResult>> Complete(Task<IReadOnlyList<CardPileAddResult>> original,
            NNagatoDrawPilePreviews.Addition addition)
        {
            var results = await original;
            NNagatoDrawPilePreviews.CompleteAddition(addition, results);
            return results;
        }
    }

    public sealed class RemovePatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_remove";
        public static string Description => "Forget drawn, exhausted and silently removed preview cards";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() => [new(typeof(CardPile), nameof(CardPile.RemoveInternal), [typeof(CardModel), typeof(bool)])];

        [HarmonyPostfix]
        public static void Postfix(CardPile __instance, CardModel card) => NNagatoDrawPilePreviews.Forget(__instance, card);
    }

    public sealed class ShufflePatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_shuffle";
        public static string Description => "Invalidate known order before the draw pile is shuffled";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() => [new(typeof(CardPileCmd), nameof(CardPileCmd.Shuffle), [typeof(PlayerChoiceContext), typeof(Player)])];

        [HarmonyPrefix]
        public static void Prefix(Player player)
        {
            if (player.PlayerCombatState is { } state)
                NNagatoDrawPilePreviews.Invalidate(state.DrawPile);
        }
    }

    public sealed class RandomizePatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_randomize";
        public static string Description => "Invalidate previews when a pile is randomized directly";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() => [new(typeof(CardPile), nameof(CardPile.RandomizeOrderInternal))];

        [HarmonyPrefix]
        public static void Prefix(CardPile __instance) => NNagatoDrawPilePreviews.Invalidate(__instance);
    }

    public sealed class MoveToTopPatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_move_top";
        public static string Description => "Record explicit in-pile top movement";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() => [new(typeof(CardPile), nameof(CardPile.MoveToTopInternal), [typeof(CardModel)])];

        [HarmonyPostfix]
        public static void Postfix(CardPile __instance, CardModel card) => NNagatoDrawPilePreviews.RememberTop(__instance, card);
    }

    public sealed class TransformPatch : IPatchMethod
    {
        public static string PatchId => "nagato_draw_pile_preview_transform";
        public static string Description => "Preserve known positions when preview cards are transformed";
        public static bool IsCritical => false;
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(CardTransformation), nameof(CardTransformation.GetReplacement), [typeof(Rng)])
        ];

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(CardTransformation __instance, CardModel? __result) =>
            NNagatoDrawPilePreviews.RememberTransformation(__instance.Original, __result);
    }
}
