using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SteadfastGuardPower : NagatoPowerModel
{
	private sealed class Data
	{
		public bool Granting;
	}

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override object InitInternalData() => new Data();
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

	public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		var data = GetInternalData<Data>();
		if (creature != Owner || amount <= 0m || data.Granting || Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		data.Granting = true;
		try
		{
			Flash();
			await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
		}
		finally
		{
			data.Granting = false;
		}
	}
}
