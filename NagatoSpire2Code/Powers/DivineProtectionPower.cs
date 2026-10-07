using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class DivineProtectionPower : NagatoPowerModel, INagatoChoiceListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DivineProtectionPlatingPower>()];

	public async Task AfterNagatoChoice(PlayerChoiceContext choiceContext)
	{
		if (Owner.Player?.PlayerCombatState is null || CombatManager.Instance.IsOverOrEnding || Owner.IsDead)
			return;

		Flash();
		await PowerCmd.Apply<DivineProtectionPlatingPower>(choiceContext, Owner, Amount, Owner, null);
	}
}
