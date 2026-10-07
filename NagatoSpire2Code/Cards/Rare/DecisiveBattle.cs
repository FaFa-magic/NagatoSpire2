using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class DecisiveBattle() : NagatoCardModel(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedEvokes").WithMultiplier((card, _) => NagatoOrbHistory.GetEvokeCount(card.Owner))
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		int count = (int)((CalculatedVar)DynamicVars["CalculatedEvokes"]).Calculate(cardPlay.Target);
		for (int i = 0; i < count; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || state.OrbQueue.Orbs.Count == 0)
				break;

			await OrbCmd.EvokeNext(choiceContext, Owner);
		}
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
