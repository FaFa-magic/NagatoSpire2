using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Rare;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmoredRefitPlatingPower : NagatoTempPowerModel<ArmoredRefit, PlatingPower>
{
	private bool _trackingTurnStart;
	private bool _updatingInternalPower;
	private int _otherPlating;

	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_ARMORED_REFIT_PLATING_POWER.title");

	public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		_otherPlating = Math.Max(target.GetPower<PlatingPower>()?.Amount ?? 0, 0);
		_trackingTurnStart = true;
		await base.BeforeApplied(target, amount, applier, cardSource);
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
		decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power == this)
		{
			_updatingInternalPower = true;
			try
			{
				await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
			}
			finally
			{
				_updatingInternalPower = false;
			}
			return;
		}

		if (!_trackingTurnStart || _updatingInternalPower || power is not PlatingPower || power.Owner != Owner)
			return;

		int change = (int)amount;
		if (change >= 0)
		{
			_otherPlating += change;
			return;
		}

		int consumed = Math.Min(_otherPlating, -change);
		_otherPlating -= consumed;
		int temporaryConsumed = Math.Min(Math.Max(Amount, 0), -change - consumed);
		if (temporaryConsumed > 0)
		{
			IgnoreNextInstance();
			await PowerCmd.ModifyAmount(choiceContext, this, -temporaryConsumed, null, null, true);
		}
	}

	public override Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(Owner))
			_trackingTurnStart = false;
		return Task.CompletedTask;
	}
}
