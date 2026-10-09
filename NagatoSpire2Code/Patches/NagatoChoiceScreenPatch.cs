using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using NagatoSpire2.NagatoSpire2Code.Cards;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoChoiceScreenPatch : IPatchMethod
{
    public static string PatchId => "nagato_choice_military_table";
    public static string Description => "Skin Nagato choice screens with the military folio and treasure-room hand";
    public static bool IsCritical => false;
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen._Ready)),
        new(typeof(NSimpleCardSelectScreen), nameof(NSimpleCardSelectScreen._Ready))
    ];

    [HarmonyPostfix]
    public static void Postfix(Control __instance, IReadOnlyList<CardModel> ____cards)
    {
        if (____cards.Count == 0 || ____cards.Any(card => card is not NagatoChoiceOption))
            return;
        // A DLL-only update must not softlock a run if its matching PCK has not been installed yet.
        if (!ResourceLoader.Exists(NagatoChoiceTable.ScenePath) || !ResourceLoader.Exists(NagatoChoiceTable.BackgroundPath))
            return;
        if (__instance is NChooseACardSelectionScreen)
        {
            // The stock opening tween otherwise keeps moving the cards away from their paper slots.
            var tween = (Tween?)AccessTools.Field(typeof(NChooseACardSelectionScreen), "_cardTween").GetValue(__instance);
            tween?.Kill();
        }
        NagatoChoiceScreenSkin.Attach(__instance, ____cards, __instance is NSimpleCardSelectScreen);
    }
}
