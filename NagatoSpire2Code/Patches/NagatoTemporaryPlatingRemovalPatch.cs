using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoTemporaryPlatingRemovalPatch : IPatchMethod
{
	public static string PatchId => "nagato_temporary_plating_removal";
	public static string Description => "Clear remaining temporary Plating when the underlying Plating is removed";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() => [new(typeof(PowerCmd), nameof(PowerCmd.Remove), [typeof(PowerModel)])];

	[HarmonyPostfix]
	public static void Postfix(PowerModel? power, ref Task __result)
	{
		if (power is PlatingPower && power.Owner.GetPower<TemporaryPlatingPower>() is { } temporary)
			__result = ClearAfterRemoval(__result, temporary);
	}

	private static async Task ClearAfterRemoval(Task removal, TemporaryPlatingPower temporary)
	{
		await removal;
		if (!temporary.Owner.HasPower<PlatingPower>())
			await temporary.Consume(new ThrowingPlayerChoiceContext(), temporary.Amount);
	}
}
