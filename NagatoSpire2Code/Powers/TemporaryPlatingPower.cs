using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class TemporaryPlatingPower : NagatoTempPowerModel<PlatingPower, PlatingPower>
{
	public override PowerAssetProfile AssetProfile => new(
		IconPath: "res://NagatoSpire2/images/powers/packed/TemporaryPlatingPower.png",
		BigIconPath: "res://NagatoSpire2/images/powers/big/TemporaryPlatingPower.png"
	);

	private int _internalChanges;

	public override LocString Title => new("powers", Id.Entry + ".title");

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
		decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power.Owner != Owner)
			return;
		if (power is PlatingPower && amount < 0 && _internalChanges == 0)
		{
			await Consume(choiceContext, (int)-amount);
			return;
		}
		if (power != this)
			return;
		_internalChanges++;
		try
		{
			await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
		}
		finally
		{
			_internalChanges--;
		}
	}

	internal Task Consume(PlayerChoiceContext choiceContext, int amount)
	{
		if (amount <= 0 || Amount <= 0 || !Owner.Powers.Contains(this))
			return Task.CompletedTask;
		IgnoreNextInstance();
		return PowerCmd.ModifyAmount(choiceContext, this, -Math.Min(amount, Amount), null, null, true);
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || !Owner.Powers.Contains(this))
			return;
		Flash();
		await PowerCmd.Remove(this);
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		if (Amount > 0 && oldOwner.GetPower<PlatingPower>() is { Amount: > 0 } plating)
			await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), plating, -Math.Min(Amount, plating.Amount), null, null);
	}
}
