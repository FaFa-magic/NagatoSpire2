using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class FireStrengthPower : NagatoTempPowerModel<FireOrb, StrengthPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_FIRE_STRENGTH_POWER.title");
}
