using MegaCrit.Sts2.Core.Entities.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class OverflowDrawPower : NagatoPowerModel
{
	protected override bool UsesPackedIcon => true;

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Single;
}
