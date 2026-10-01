using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

[RegisterPower]
public sealed class ArmorPiercingFocusPower : ModTemporaryAppliedPowerTemplate<ArmorPiercingShellOrb, FocusPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_ARMOR_PIERCING_FOCUS_POWER.title");
	public override PowerAssetProfile AssetProfile => new(
		IconPath: "res://images/powers/focus_power.png",
		BigIconPath: "res://images/powers/focus_power.png");
}
