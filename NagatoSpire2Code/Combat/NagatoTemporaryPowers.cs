using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoTemporaryPowers
{
	public static decimal Count(Creature creature, Type? internalPowerType = null) => creature.Powers
		.Where(power => power is ITemporaryPower temporary &&
			(internalPowerType == null || temporary.InternallyAppliedPower.GetType() == internalPowerType))
		.Sum(power => Math.Abs((decimal)power.Amount));
}
