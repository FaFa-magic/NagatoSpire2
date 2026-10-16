using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

[RegisterPower(Inherit = true)]
public abstract class NagatoPowerModel : ModPowerTemplate
{
	protected virtual bool UsesPackedIcon => false;

	public override PowerAssetProfile AssetProfile => new(
		IconPath: $"res://NagatoSpire2/images/powers/{(UsesPackedIcon ? "packed" : "big")}/{GetType().Name}.png",
		BigIconPath: $"res://NagatoSpire2/images/powers/big/{GetType().Name}.png"
	);
}
