using MegaCrit.Sts2.Core.Entities.Relics;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

[RegisterCharacterStarterRelic(typeof(NagatoCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(BigShipsBigGunsDivineMight))]
public sealed class BigShipsBigGuns : NagatoShellRefillRelic
{
	public override RelicRarity Rarity => RelicRarity.Starter;
}
