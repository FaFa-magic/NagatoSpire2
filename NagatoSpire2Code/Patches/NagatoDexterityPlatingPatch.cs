using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoDexterityPlatingPatch : IPatchMethod
{
	public static string PatchId => "nagato_dexterity_temporary_plating";
	public static string Description => "Extend Dexterity's additive bonus to temporary Plating granted by cards";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(AbstractModel), nameof(AbstractModel.ModifyPowerAmountGivenAdditive))];

	[HarmonyPostfix]
	public static void Postfix(AbstractModel __instance, PowerModel power, Creature giver, decimal amount,
		CardModel? cardSource, ref decimal __result)
	{
		if (__instance is DexterityPower dexterity && dexterity.Owner == giver &&
			power is TemporaryPlatingPower && amount > 0m && cardSource?.Owner.Creature == giver)
			__result += Math.Max(amount + dexterity.Amount, 0m) - amount;
	}
}
