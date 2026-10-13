using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoOrbCardSlotDisplayPatch : IPatchMethod
{
	public static string PatchId => "nagato_orb_card_slot_display";
	public static string Description => "Display each player's draw-pile cards beneath their horizontal orb slots";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NOrbManager), "TweenLayout", Type.EmptyTypes)
	];

	[HarmonyPrefix]
	public static bool Prefix(NOrbManager __instance) => !NNagatoOrbCardSlots.TryLayout(__instance);
}
