using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class CalamityPurificationPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (Owner.Player?.PlayerCombatState is not { } state)
			return;
		foreach (var card in state.AllCards.ToArray())
			await SealStatus(card);
	}

	public override Task AfterCardEnteredCombat(CardModel card) => SealStatus(card);

	public override Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw) => SealStatus(card);

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner != Owner.Player || card.Affliction is not Sealed || Owner.IsDead ||
			CombatManager.Instance.IsOverOrEnding || Owner.Player is not { PlayerCombatState: not null } player)
			return;
		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player);
	}

	private async Task SealStatus(CardModel card)
	{
		if (card.Owner != Owner.Player || card.Type != CardType.Status || card.Affliction is Sealed ||
			card.HasBeenRemovedFromState || Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		if (card.Affliction != null)
			CardCmd.ClearAffliction(card);
		await CardCmd.Afflict<Sealed>(card, 1m);
	}
}
