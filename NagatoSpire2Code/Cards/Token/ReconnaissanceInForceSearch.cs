using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ReconnaissanceInForceSearch : NagatoChoiceOption
{
	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		var selected = (await CardSelectCmd.FromCombatPile(choiceContext, state.DrawPile, Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		if (selected?.Pile == state.DrawPile)
			await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
	}

	protected override void OnUpgrade() { }
}
