using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Rewards;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class FlagshipCommandPower : NagatoPowerModel
{
	private bool _claimed;

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override bool UsesPackedIcon => true;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
		DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		var killer = dealer?.Player ?? dealer?.PetOwner;
		if (target != Owner || !result.WasTargetKilled || _claimed || killer == null ||
			killer.Creature.Side == Owner.Side || killer.Creature.IsDead ||
			Owner.Powers.Any(power => !power.ShouldOwnerDeathTriggerFatal()))
			return;
		_claimed = true;
		int count = Amount;
		Flash();
		await CreatureCmd.GainMaxHp(killer.Creature, 2 * count);
		if (Owner.CombatState?.RunState.CurrentRoom is CombatRoom room)
			for (int i = 0; i < count; i++)
				room.AddExtraReward(killer, new CardUpgradeReward(killer));
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState) =>
		side == CombatSide.Enemy ? PowerCmd.Remove(this) : Task.CompletedTask;
}
