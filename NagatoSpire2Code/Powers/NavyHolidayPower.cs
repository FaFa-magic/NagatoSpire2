using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class NavyHolidayPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.GetPower<PlatingPower>() == null)
			return;

		Flash();
		int repeats = Amount;
		for (int i = 0; i < repeats; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead || Owner.GetPower<PlatingPower>() is not { } plating)
				break;

			await plating.BeforeSideTurnEndEarly(choiceContext, side, participants);
		}
	}
}
