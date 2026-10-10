using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Patches;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class LockOnPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
		Creature? dealer, CardModel? cardSource, CardPlay? cardPlay) =>
		target == Owner && dealer != null && NagatoOrbDamagePatch.CurrentOrb?.Owner.Creature == dealer &&
		cardSource == null && cardPlay == null ? 1.5m : 1m;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == Owner.Side)
			await PowerCmd.TickDownDuration(this);
	}
}
