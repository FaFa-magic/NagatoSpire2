using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoCardOrbVisualPatch : IPatchMethod
{
	public static string PatchId => "nagato_card_orb_visual";
	public static string Description => "Show the bound native orb at the top right of its card";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCard), nameof(NCard.UpdateVisuals), [typeof(PileType), typeof(CardPreviewMode)])
	];

	[HarmonyPostfix]
	public static void Postfix(NCard __instance) => NNagatoCardOrb.Refresh(__instance);
}
