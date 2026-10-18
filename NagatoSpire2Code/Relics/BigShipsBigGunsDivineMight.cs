using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

public sealed class BigShipsBigGunsDivineMight : LoadingRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;
	protected override OrbModel ShellOrb => ModelDb.Orb<ThunderOrb>();
}
