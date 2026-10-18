using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SalvoFirePower : NagatoPowerModel, INagatoSalvoListener
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override bool UsesPackedIcon => true;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Salvo, HoverTipFactory.FromPower<PlatingPower>()];

	public async Task AfterNagatoSalvo(PlayerChoiceContext choiceContext)
	{
		if (Owner.Player?.PlayerCombatState is not { } state || Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		Flash();
		await PowerCmd.Apply<TemporaryPlatingPower>(choiceContext, Owner, Amount, Owner, null);
		if (!Owner.IsDead && !CombatManager.Instance.IsOverOrEnding && Owner.Player.PlayerCombatState == state)
			await NagatoPlatingCmd.Trigger(choiceContext, Owner.Player);
	}
}
