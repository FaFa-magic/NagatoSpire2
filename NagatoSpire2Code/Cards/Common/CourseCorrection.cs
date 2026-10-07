using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class CourseCorrection() : NagatoCardModel(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new IntVar("PutBack", 1)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		var selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars["PutBack"].IntValue), filter: null, source: this)).ToArray();
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		var cards = selected.Where(card => card.Owner == Owner && card.Pile == state.Hand).Distinct().ToArray();
		if (cards.Length > 0)
			await CardPileCmd.Add(cards, state.DrawPile, CardPilePosition.Top);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
		DynamicVars["PutBack"].UpgradeValueBy(1m);
	}
}
