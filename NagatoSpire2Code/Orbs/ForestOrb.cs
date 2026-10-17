using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class ForestOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#3b6750");
	public override decimal PassiveVal => 1m;
	public override decimal EvokeVal => 1m;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SingleEvoke;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<FocusPower>()];

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, EvokeVal, Owner.Creature, null);
		return [Owner.Creature];
	}
}
