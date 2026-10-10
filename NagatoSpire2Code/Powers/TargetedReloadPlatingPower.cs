using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Basic;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class TargetedReloadPlatingPower : NagatoTempPowerModel<TargetedReload, PlatingPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_TARGETED_RELOAD_PLATING_POWER.title");
}
