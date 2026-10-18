using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class ShadowOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#5c4876");
	public override decimal PassiveVal => 1m;
	public override decimal EvokeVal => FocusValue(1m);
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SingleEvoke;
	public override bool IsFocusAffected => true;

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		await PlayerCmd.GainEnergy(EvokeVal, Owner);
		return [Owner.Creature];
	}
}
