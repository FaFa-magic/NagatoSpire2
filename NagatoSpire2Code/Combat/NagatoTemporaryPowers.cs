using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoTemporaryPowers
{
	public static decimal Count(Creature creature) => creature.Powers
		.Where(power => power is ITemporaryPower)
		.Sum(power => Math.Abs((decimal)power.Amount));

	public static async Task Reduce(Creature creature, int amount)
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

			int reduction = Math.Min(Math.Abs(power.Amount), amount);
			power.SetAmount(power.Amount - Math.Sign(power.Amount) * reduction);
			if (power.ShouldRemoveDueToAmount())
				await PowerCmd.Remove(power);
		}
	}
}
