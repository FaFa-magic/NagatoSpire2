using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Afflictions;

[RegisterAffliction(Inherit = true)]
public abstract class NagatoAfflictionModel : ModAfflictionTemplate
{
	public override AfflictionAssetProfile AssetProfile => new(
		OverlayScenePath: $"res://NagatoSpire2/scenes/cards/overlays/afflictions/{GetType().Name}.tscn"
	);
}
