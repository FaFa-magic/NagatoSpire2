using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoTemporaryPowers
{
	public static decimal Count(Creature creature, Type? internalPowerType = null) => creature.Powers
		.Where(power => power is ITemporaryPower temporary &&
			(internalPowerType == null || temporary.InternallyAppliedPower.GetType() == internalPowerType))
		.Sum(power => Math.Abs((decimal)power.Amount));

	public static async Task Reduce(PlayerChoiceContext choiceContext, Creature creature, int amount, CardModel source)
	{
		if (amount <= 0)
			return;

		var powers = creature.Powers.Where(power => power is ITemporaryPower).ToArray();
		foreach (var power in powers)
		{
			if (CombatManager.Instance.IsOverOrEnding || creature.IsDead)
				break;
			if (!creature.Powers.Contains(power) || power.Amount == 0)
				continue;

			decimal reduction = Math.Min(Math.Abs((decimal)power.Amount), amount);
			await PowerCmd.ModifyAmount(choiceContext, power, -Math.Sign(power.Amount) * reduction, null, source);
		}
	}
}
