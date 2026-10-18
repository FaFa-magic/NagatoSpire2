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
	private sealed class Data
	{
		public bool TriggeredThisTurn;
	}

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override object InitInternalData() => new Data();
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (Owner.Player?.PlayerCombatState is not { } state)
			return;
		foreach (var card in state.AllCards.ToArray())
			await SealCard(card);
	}

	public override Task AfterCardEnteredCombat(CardModel card) => SealCard(card);

	public override Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw) => SealCard(card);

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == CombatSide.Player && participants.Contains(Owner))
			GetInternalData<Data>().TriggeredThisTurn = false;
		return Task.CompletedTask;
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner != Owner.Player || card.Affliction is not Sealed || Owner.IsDead ||
			CombatManager.Instance.IsOverOrEnding || Owner.Player is not { PlayerCombatState: not null } player)
			return;
		var data = GetInternalData<Data>();
		if (data.TriggeredThisTurn)
			return;
		data.TriggeredThisTurn = true;
		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player);
	}

	private async Task SealCard(CardModel card)
	{
		if (card.Owner != Owner.Player || card.Type is not (CardType.Status or CardType.Curse) || card.Affliction is Sealed ||
			card.HasBeenRemovedFromState || Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		if (card.Affliction != null)
			CardCmd.ClearAffliction(card);
		await CardCmd.Afflict<Sealed>(card, 1m);
	}
}
