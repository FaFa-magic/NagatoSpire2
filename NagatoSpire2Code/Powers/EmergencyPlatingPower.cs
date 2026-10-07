using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Common;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class EmergencyPlatingPower : NagatoTempPowerModel<EmergencyPlating, PlatingPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_EMERGENCY_PLATING_POWER.title");
}
