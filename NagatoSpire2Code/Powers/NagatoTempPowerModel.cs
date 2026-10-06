using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

[RegisterPower(Inherit = true)]
public abstract class NagatoTempPowerModel<TOriginModel, TPower> : ModTemporaryAppliedPowerTemplate<TOriginModel, TPower>
	where TOriginModel : AbstractModel
	where TPower : PowerModel
{
	public override PowerAssetProfile AssetProfile => new(
		IconPath: $"res://NagatoSpire2/images/powers/big/{GetType().Name}.png",
		BigIconPath: $"res://NagatoSpire2/images/powers/big/{GetType().Name}.png"
	);
}
