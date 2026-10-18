using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Combat.HandSize;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class StillWatersPower : NagatoPowerModel, IMaxHandSizeModifier
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyHandDraw(Player player, decimal count) => player == Owner.Player ? count + Amount : count;

	public int ModifyMaxHandSize(Player player, int currentMaxHandSize) =>
		player == Owner.Player ? currentMaxHandSize - Amount : currentMaxHandSize;
}
