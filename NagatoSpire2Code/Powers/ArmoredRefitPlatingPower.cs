using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Rare;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmoredRefitPlatingPower : NagatoTempPowerModel<ArmoredRefit, PlatingPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_ARMORED_REFIT_PLATING_POWER.title");
}
