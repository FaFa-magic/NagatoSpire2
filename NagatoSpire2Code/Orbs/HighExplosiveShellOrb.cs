using Godot;
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
	public override Color DarkenedColor => new("#8B3938");
	public override OrbAssetProfile AssetProfile => new(
		IconPath: "res://NagatoSpire2/images/orbs/high_explosive_shell_orb.png",
		VisualsScenePath: "res://NagatoSpire2/scenes/orbs/high_explosive_shell_orb.tscn");

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
