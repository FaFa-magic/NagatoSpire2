using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class DefensiveFire() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override bool GainsBlock => true;

	public override OrbEvokeType OrbEvokeType => OrbEvokeType.Front;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new RepeatVar(1)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
				Owner.PlayerCombatState != state || state.OrbQueue.Orbs.Count == 0)
				break;

			await OrbCmd.EvokeNext(choiceContext, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(2m);
		DynamicVars.Repeat.UpgradeValueBy(1m);
	}
}
