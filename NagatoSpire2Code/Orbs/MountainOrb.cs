using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class MountainOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#816a48");
	public override decimal PassiveVal => FocusValue(4m);
	public override decimal EvokeVal => PassiveVal;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.Contextual;
	public override bool IsFocusAffected => true;
	protected override bool HasPlayPassive => true;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		ActivatePassive();
		await PowerCmd.Apply<MountainPlatingPower>(choiceContext, Owner.Creature, PassiveVal, Owner.Creature, null);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, EvokeVal, Owner.Creature, null);
		return [Owner.Creature];
	}
}
