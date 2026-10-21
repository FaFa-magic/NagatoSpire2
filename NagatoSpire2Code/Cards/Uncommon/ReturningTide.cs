using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ReturningTide() : NagatoCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move), new CardsVar(5)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (Owner.PlayerCombatState is not { } state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		var drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner)).ToArray();
		if (Owner.PlayerCombatState != state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int count = drawn.Distinct().Count(card => card.Owner == Owner && card.Pile == state.Hand);
		if (count == 0)
			return;
		var selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, count), filter: null, source: this)).ToArray();
		if (Owner.PlayerCombatState != state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		var cards = selected.Where(card => card.Owner == Owner && card.Pile == state.Hand).Distinct().ToArray();
		if (cards.Length > 0)
			await CardPileCmd.Add(cards, state.DrawPile, CardPilePosition.Random);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
