using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmorSuperiorityPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
		Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if ((dealer != Owner && (dealer == null || !Owner.Pets.Contains(dealer))) || Owner.MaxHp <= 0 || Amount <= 0)
			return 1m;

		return 1m + (decimal)Math.Max(Owner.GetPower<PlatingPower>()?.Amount ?? 0, 0) / Owner.MaxHp * Amount;
	}
}
