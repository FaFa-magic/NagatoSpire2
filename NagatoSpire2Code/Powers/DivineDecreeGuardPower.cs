using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Cards.Rare;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class DivineDecreeGuardPower : NagatoPowerModel, ITemporaryPower
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Single;
	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
	public AbstractModel OriginModel => ModelDb.Card<DivineDecree>();
	public PowerModel InternallyAppliedPower => ModelDb.Power<DivineDecreeGuardPower>();

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
		Creature? dealer, CardModel? cardSource, CardPlay? cardPlay) => target == Owner ? 0.5m : 1m;

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants) =>
		participants.Contains(Owner) ? PowerCmd.Remove(this) : Task.CompletedTask;
}
