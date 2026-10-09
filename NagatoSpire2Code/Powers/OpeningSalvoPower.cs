using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class OpeningSalvoPower : NagatoPowerModel
{
	private bool _triggeredThisTurn;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(Owner))
			_triggeredThisTurn = false;
		return Task.CompletedTask;
	}

	public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
	{
		if (orb.Owner != Owner.Player || _triggeredThisTurn || Owner.Player is not { PlayerCombatState: not null } player ||
			CombatManager.Instance.IsOverOrEnding || Owner.IsDead)
			return;

		_triggeredThisTurn = true;
		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player);
	}
}
