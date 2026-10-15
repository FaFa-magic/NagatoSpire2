using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoPlatingTurnStartPatch : IPatchMethod
{
	public static string PatchId => "nagato_plating_turn_start";
	public static string Description => "Remove turn-start Plating decay";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PlatingPower), nameof(PlatingPower.AfterSideTurnStart),
			[typeof(CombatSide), typeof(IReadOnlyList<Creature>), typeof(ICombatState)])
	];

	[HarmonyPrefix]
	public static bool Prefix(ref Task __result)
	{
		__result = Task.CompletedTask;
		return false;
	}
}
