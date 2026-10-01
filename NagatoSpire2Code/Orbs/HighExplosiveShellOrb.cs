using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

[RegisterOrb]
public sealed class HighExplosiveShellOrb : NagatoShellOrb
{
	public override decimal EvokeVal => ModifyOrbValue(3m);
	public override OrbAssetProfile AssetProfile => new(
		IconPath: "res://images/orbs/lightning_orb.png",
		VisualsScenePath: "res://scenes/orbs/orb_visuals/lightning_orb.tscn");

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		Creature[] targets = CombatState.GetOpponentsOf(Owner.Creature)
			.Where(creature => creature.IsHittable)
			.ToArray();
		ActivateEvoke(targets);
		PlayEvokeSfx();
		if (targets.Length > 0)
			await CreatureCmd.Damage(choiceContext, targets, EvokeVal, ValueProp.Unpowered, Owner.Creature);
		return targets;
	}
}
