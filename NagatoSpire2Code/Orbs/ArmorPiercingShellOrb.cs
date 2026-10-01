using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

[RegisterOrb]
public sealed class ArmorPiercingShellOrb : NagatoShellOrb
{
	public override decimal EvokeVal => ModifyOrbValue(3m);
	public override OrbAssetProfile AssetProfile => new(
		IconPath: "res://images/orbs/dark_orb.png",
		VisualsScenePath: "res://scenes/orbs/orb_visuals/dark_orb.tscn");

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		Creature? target = CombatState.GetOpponentsOf(Owner.Creature)
			.Where(creature => creature.IsHittable)
			.OrderByDescending(creature => creature.CurrentHp)
			.FirstOrDefault();
		Creature[] targets = target is null ? [] : [target];
		ActivateEvoke(targets);
		PlayEvokeSfx();
		if (target is not null)
			await CreatureCmd.Damage(choiceContext, target, EvokeVal, ValueProp.Unpowered, Owner.Creature);
		await PowerCmd.Apply<ArmorPiercingFocusPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
		return targets;
	}
}
