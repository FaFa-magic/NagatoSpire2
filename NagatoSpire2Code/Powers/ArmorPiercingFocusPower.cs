using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmorPiercingFocusPower : NagatoTempPowerModel<ArmorPiercingShellOrb, FocusPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_ARMOR_PIERCING_FOCUS_POWER.title");
}
