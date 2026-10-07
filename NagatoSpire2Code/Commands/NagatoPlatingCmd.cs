using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoPlatingCmd
{
	public static async Task Trigger(PlayerChoiceContext choiceContext, Player player)
	{
		if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead ||
			player.Creature.GetPower<PlatingPower>() is not { Amount: > 0 } plating)
			return;

		await plating.BeforeSideTurnEndEarly(choiceContext, player.Creature.Side, [player.Creature]);
		if (!CombatManager.Instance.IsOverOrEnding && !player.Creature.IsDead &&
			player.Creature.GetPower<NavyHolidayPower>() is { } holiday)
			await holiday.BeforeSideTurnEndEarly(choiceContext, player.Creature.Side, [player.Creature]);
	}
}
