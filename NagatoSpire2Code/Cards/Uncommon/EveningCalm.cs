using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class EveningCalm() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		var cards = state.Hand.Cards.Reverse().ToArray();
		if (cards.Length == 0)
			return;
		var moved = await CardPileCmd.Add(cards, state.DrawPile, CardPilePosition.Top);
		int count = moved.Count(result => result.success);
		if (count > 0 && Owner.PlayerCombatState == state && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await PowerCmd.Apply<EveningCalmPower>(choiceContext, Owner.Creature, count, Owner.Creature, this);
	}

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
