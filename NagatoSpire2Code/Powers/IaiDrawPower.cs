using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class IaiDrawPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.Player?.PlayerCombatState == null)
			return;

		int count = Amount;
		Flash();
		await PowerCmd.Remove(this);
		await CardPileCmd.Draw(choiceContext, count, Owner.Player);
	}
}
