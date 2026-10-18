using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ExhaustHandAtTurnEndPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Single;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.IsDead ||
			CombatManager.Instance.IsOverOrEnding || Owner.Player is not { PlayerCombatState: { } state } player)
			return;
		var cards = state.Hand.Cards.ToArray();
		Flash();
		await PowerCmd.Remove(this);
		foreach (var card in cards)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead || player.PlayerCombatState != state)
				break;
			if (!card.HasBeenRemovedFromState && card.Owner == player && card.Pile == state.Hand)
				await CardCmd.Exhaust(choiceContext, card);
		}
	}
}
