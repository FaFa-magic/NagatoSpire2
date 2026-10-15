using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Combat.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoPlatingCmd
{
	private static readonly Action<PowerModel> FlashPower = AccessTools.MethodDelegate<Action<PowerModel>>(
		AccessTools.DeclaredMethod(typeof(PowerModel), "Flash"));

	public static async Task Trigger(PlayerChoiceContext choiceContext, Player player)
	{
		if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead ||
			player.Creature.GetPower<PlatingPower>() is not { Amount: > 0 } plating)
			return;

		await Trigger(choiceContext, plating);
	}

	internal static async Task Trigger(PlayerChoiceContext choiceContext, PlatingPower plating)
	{
		var owner = plating.Owner;
		var combatState = owner.CombatState;
		var holiday = owner.GetPower<NavyHolidayPower>();
		int repeats = 1 + Math.Min(Math.Max(holiday?.Amount ?? 0, 0), Math.Max(plating.Amount - 1, 0));
		if (repeats > 1)
			FlashPower(holiday!);
		for (int i = 0; i < repeats; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || owner.IsDead || owner.CombatState != combatState ||
				plating.Amount <= 0 || !owner.Powers.Contains(plating))
				break;
			int amount = plating.Amount;
			FlashPower(plating);
			await CreatureCmd.GainBlock(owner, amount, ValueProp.Unpowered, null);
			if (owner.GetPower<ArmoredReprisalPower>() is { } reprisal)
				await reprisal.AfterPlatingTriggered(choiceContext, amount);
			if (CombatManager.Instance.IsOverOrEnding || owner.IsDead || owner.CombatState != combatState ||
				!owner.Powers.Contains(plating))
				break;
			int before = plating.Amount;
			await PowerCmd.ModifyAmount(choiceContext, plating, -1m, null, null);
			int consumed = Math.Min(1, Math.Max(before - plating.Amount, 0));
			foreach (var temporary in owner.Powers.OfType<ModTemporaryPowerTemplate>()
				.Where(power => power.InternallyAppliedPower is PlatingPower && power.Amount > 0).ToArray())
			{
				if (consumed <= 0)
					break;
				int decrement = Math.Min(consumed, temporary.Amount);
				temporary.IgnoreNextInstance();
				await PowerCmd.ModifyAmount(choiceContext, temporary, -decrement, null, null, true);
				consumed -= decrement;
			}
		}
	}
}
