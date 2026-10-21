using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Rewards;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class NagatosEdictPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	public override Task AfterCombatEnd(CombatRoom room)
	{
		if (Owner.Player is { } player && !Owner.IsDead)
			for (int i = 0; i < Amount; i++)
				room.AddExtraReward(player, new CardSealReward(player));
		return Task.CompletedTask;
	}
}
