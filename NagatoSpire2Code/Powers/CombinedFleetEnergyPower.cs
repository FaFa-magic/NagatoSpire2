using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class CombinedFleetEnergyPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.ForEnergy(this)];
	public override decimal ModifyMaxEnergy(Player player, decimal amount) => player == Owner.Player ? amount + Amount : amount;
}
