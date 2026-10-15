using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Commands;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoPlatingTriggerPatch : IPatchMethod
{
	public static string PatchId => "nagato_plating_trigger";
	public static string Description => "Resolve Plating triggers and decrement after each activation";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PlatingPower), nameof(PlatingPower.BeforeSideTurnEndEarly),
			[typeof(PlayerChoiceContext), typeof(CombatSide), typeof(IEnumerable<Creature>)])
	];

	[HarmonyPrefix]
	public static bool Prefix(PlatingPower __instance, PlayerChoiceContext choiceContext,
		IEnumerable<Creature> participants, ref Task __result)
	{
		__result = participants.Contains(__instance.Owner)
			? NagatoPlatingCmd.Trigger(choiceContext, __instance) : Task.CompletedTask;
		return false;
	}
}
