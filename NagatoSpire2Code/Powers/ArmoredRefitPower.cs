using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmoredRefitPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner) || Owner.Player?.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.IsDead || Owner.Block <= 0)
			return;

		int block = Owner.Block;
		Flash();
		await CreatureCmd.LoseBlock(choiceContext, Owner, block, Owner);
		if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead || Owner.Player.PlayerCombatState != state)
			return;

		await PowerCmd.Apply<ArmoredRefitPlatingPower>(choiceContext, Owner, block, Owner, null);
	}
}
