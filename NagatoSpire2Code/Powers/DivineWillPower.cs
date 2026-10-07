using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class DivineWillPower : NagatoPowerModel, INagatoChoiceListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	public async Task AfterNagatoChoice(PlayerChoiceContext choiceContext)
	{
		if (Owner.Player is not { PlayerCombatState: { } state } player)
			return;

		int count = Amount;
		for (int i = 0; i < count; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead || player.PlayerCombatState != state || state.OrbQueue.Orbs.Count == 0)
				break;

			Flash();
			await OrbCmd.EvokeNext(choiceContext, player);
		}
	}
}
