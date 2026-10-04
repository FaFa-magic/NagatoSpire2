using Godot;
using NagatoSpire2.NagatoSpire2Code.Audio;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public abstract class NagatoShellOrb : ModOrbTemplate
{
	public override decimal PassiveVal => 0m;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SingleEvoke;
	public override bool AllowInRandomOrbPool => false;
	public override Color DarkenedColor => new("#624B63");
	protected override string PassiveSfx => NagatoAudio.OrbPassiveEvent;
	protected override string ChannelSfx => NagatoAudio.OrbChannelEvent;
	protected override string EvokeSfx => NagatoAudio.OrbEvokeEvent;

	public static OrbModel CreateRandom(Player player)
	{
		OrbModel[] shells =
		[
			ModelDb.Orb<HighExplosiveShellOrb>(),
			ModelDb.Orb<ArmorPiercingShellOrb>(),
			ModelDb.Orb<TypeThreeShellOrb>()
		];
		return player.RunState.Rng.CombatOrbGeneration.NextItem(shells)!.ToMutable();
	}
}
