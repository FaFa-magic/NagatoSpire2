using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class NavyHolidayPower : NagatoPowerModel
{
	protected override bool UsesPackedIcon => true;

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];
}
