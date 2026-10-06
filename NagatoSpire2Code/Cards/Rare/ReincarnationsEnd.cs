using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class ReincarnationsEnd() : NagatoCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromPower<ReincarnationsEndPower>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
			return;

		int count = Math.Max(0, CardPile.MaxCardsInHand - state.Hand.Cards.Count);
		await CardPileCmd.Draw(choiceContext, count, Owner);
		if (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await PowerCmd.Apply<ReincarnationsEndPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
