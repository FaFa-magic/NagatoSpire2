using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class TripleVolleyPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => DynamicVars["OrbsLeft"].IntValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("OrbsLeft", 3)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
	{
		if (orb.Owner != Owner.Player || Owner.Player?.PlayerCombatState is null ||
			CombatManager.Instance.IsOverOrEnding || Owner.IsDead)
			return;

		DynamicVars["OrbsLeft"].BaseValue--;
		if (DynamicVars["OrbsLeft"].IntValue > 0)
		{
			InvokeDisplayAmountChanged();
			return;
		}

		DynamicVars["OrbsLeft"].BaseValue = 3m;
		InvokeDisplayAmountChanged();
		Flash();
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, Amount, Owner, null);
	}
}
