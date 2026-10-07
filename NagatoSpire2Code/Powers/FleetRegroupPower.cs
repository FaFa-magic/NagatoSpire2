using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class FleetRegroupPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Single;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.IsDead || CombatManager.Instance.IsOverOrEnding ||
			Owner.Player is not { PlayerCombatState: { } state } player || state.DiscardPile.Cards.Count == 0)
			return;

		Flash();
		CardPile discard = state.DiscardPile;
		var selected = (await CardSelectCmd.FromCombatPile(choiceContext, discard, player,
			new CardSelectorPrefs(SelectionScreenPrompt, 0, discard.Cards.Count))).ToArray();
		if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState != state)
			return;

		var cards = selected.Distinct().Where(card => card.Owner == player && card.Pile == discard).ToArray();
		if (cards.Length > 0)
			await CardPileCmd.Add(cards, state.DrawPile, CardPilePosition.Random);
	}
}
