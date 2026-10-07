using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class DivineProtectionPlatingPower : NagatoTempPowerModel<DivineProtection, PlatingPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_DIVINE_PROTECTION_PLATING_POWER.title");
}
