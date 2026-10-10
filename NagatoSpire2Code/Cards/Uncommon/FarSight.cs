using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class FarSight() : NagatoCardModel(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	protected override bool HasEnergyCostX => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int x = ResolveEnergyXValue();
		var cards = (await CardPileCmd.Draw(choiceContext, x, Owner)).ToArray();
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		foreach (var card in cards)
			if (!card.HasBeenRemovedFromState && !card.EnergyCost.CostsX && card.EnergyCost.Canonical >= 0)
				card.EnergyCost.AddThisTurn(-x);
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
