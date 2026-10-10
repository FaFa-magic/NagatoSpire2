using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class CombinedFleetDrawPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public override decimal ModifyHandDraw(Player player, decimal count) => player == Owner.Player ? count + Amount : count;
}
