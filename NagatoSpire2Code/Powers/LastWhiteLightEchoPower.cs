using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Rare;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class LastWhiteLightEchoPower : NagatoPowerModel, ITemporaryPower
{
	protected override bool UsesPackedIcon => true;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public AbstractModel OriginModel => ModelDb.Card<LastWhiteLight>();

	public PowerModel InternallyAppliedPower => ModelDb.Power<LastWhiteLightEchoPower>();

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
		card.Owner.Creature == Owner ? playCount + Amount : playCount;

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (participants.Contains(Owner))
			await PowerCmd.Remove(this);
	}
}
