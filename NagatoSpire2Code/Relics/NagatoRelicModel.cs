using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

[RegisterRelic(typeof(NagatoRelicPool), Inherit = true)]
public abstract class NagatoRelicModel : ModRelicTemplate
{
	public override RelicAssetProfile AssetProfile => new(
		IconPath: $"res://NagatoSpire2/images/relics/packed/{GetType().Name}.png",
		IconOutlinePath: $"res://NagatoSpire2/images/relics/outline/{GetType().Name}.png",
		BigIconPath: $"res://NagatoSpire2/images/relics/big/{GetType().Name}.png"
	);
}
