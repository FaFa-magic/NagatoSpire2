using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class LastWhiteLightPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		NagatoWhiteLightVfx.EnsureAura(Owner);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		NagatoWhiteLightVfx.FadeAura(oldOwner);
		return Task.CompletedTask;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
		card.Owner.Creature == Owner ? playCount + Math.Max(Amount - 1, 0) : playCount;

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner) || Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;

		Flash();
		await NagatoWhiteLightVfx.Detonate(Owner, () => PowerCmd.Remove(this));
		// Visual waits may outlive the room or the owner; never damage a new/finished combat.
		if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || !ReferenceEquals(Owner.CombatState, combatState))
			return;
		await CreatureCmd.Damage(choiceContext, Owner, 999999999m,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null, null);
	}
}
