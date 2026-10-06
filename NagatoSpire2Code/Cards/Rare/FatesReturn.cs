using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class FatesReturn() : NagatoCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
			return;

		CardPile exhaust = state.ExhaustPile;
		var selected = (await CardSelectCmd.FromCombatPile(
			choiceContext,
			exhaust,
			Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1),
			card => card.Id != Id)).FirstOrDefault();
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		if (selected?.Pile == exhaust)
			await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
		if (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
	}

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
