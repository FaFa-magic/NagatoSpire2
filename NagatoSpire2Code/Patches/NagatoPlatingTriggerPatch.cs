using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoPlatingTriggerPatch : IPatchMethod
{
	public static string PatchId => "nagato_plating_trigger";
	public static string Description => "Resolve Nagato's damage after each Plating trigger";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PlatingPower), nameof(PlatingPower.BeforeSideTurnEndEarly),
			[typeof(PlayerChoiceContext), typeof(CombatSide), typeof(IEnumerable<Creature>)])
	];

	[HarmonyPrefix]
	public static void Prefix(PlatingPower __instance, IEnumerable<Creature> participants, out TriggerState? __state)
	{
		__state = null;
		if (__instance.Amount > 0 && participants.Contains(__instance.Owner) &&
			__instance.Owner.GetPower<ArmoredReprisalPower>() is { } power)
			__state = new TriggerState(power, __instance.Amount);
	}

	[HarmonyPostfix]
	public static void Postfix(PlayerChoiceContext choiceContext, TriggerState? __state, ref Task __result)
	{
		if (__state != null)
			__result = AfterTrigger(__result, choiceContext, __state);
	}

	private static async Task AfterTrigger(Task trigger, PlayerChoiceContext choiceContext, TriggerState state)
	{
		await trigger;
		if (state.Power.Owner.Powers.Contains(state.Power))
			await state.Power.AfterPlatingTriggered(choiceContext, state.PlatingAmount);
	}

	public sealed record TriggerState(ArmoredReprisalPower Power, int PlatingAmount);
}
