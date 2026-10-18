using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class GunneryManualPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override bool UsesPackedIcon => true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.FromCard<Shell>(), HoverTipFactory.FromPower<WeakPower>()];

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
		Creature? dealer, CardModel? card, CardPlay? cardPlay) =>
		dealer == Owner && props.IsPoweredAttack() && card is Shell ? Amount : 0m;
}
