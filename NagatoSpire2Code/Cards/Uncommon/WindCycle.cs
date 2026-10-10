using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class WindCycle() : NagatoCardModel(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		await CardPileCmd.Add(state.Hand.Cards.ToArray(), state.DrawPile, CardPilePosition.Random);
		if (Owner.PlayerCombatState == state && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
	}

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
