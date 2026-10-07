using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SalvoFirePower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Evoke),
		HoverTipFactory.FromOrb<HighExplosiveShellOrb>()
	];

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.Player?.PlayerCombatState is not { } state)
			return;

		int repeats = Amount;
		for (int i = 0; i < repeats; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead)
				break;
			HighExplosiveShellOrb[] shells = state.OrbQueue.Orbs.OfType<HighExplosiveShellOrb>().ToArray();
			if (shells.Length == 0)
				break;

			Flash();
			await NagatoOrbCmd.Evoke(choiceContext, Owner.Player, shells);
		}
	}
}
