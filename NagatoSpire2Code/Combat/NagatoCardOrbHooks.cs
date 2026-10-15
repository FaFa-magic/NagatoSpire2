using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

[RegisterSingleton]
public sealed class NagatoCardOrbHooks() : HookedSingletonModel(HookType.Combat)
{
	public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal) =>
		NagatoCardOrbs.Evoke(choiceContext, card);

	public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) =>
		Trigger(choiceContext, player, true);

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player)
			return;
		foreach (var creature in participants)
			if (creature.Player is { } player)
				await Trigger(choiceContext, player, false);
	}

	private static async Task Trigger(PlayerChoiceContext choiceContext, Player player, bool turnStart)
	{
		var combat = player.PlayerCombatState;
		foreach (var (card, orb) in NagatoCardOrbs.GetHand(player))
		{
			if (CombatManager.Instance.IsOverOrEnding || !player.IsActiveForHooks || player.PlayerCombatState != combat)
				break;
			if (card.Pile == combat?.Hand && NagatoCardOrbs.Get(card) == orb)
			{
				choiceContext.PushModel(orb);
				try
				{
					if (turnStart)
						await orb.AfterTurnStartOrbTrigger(choiceContext);
					else
						await orb.BeforeTurnEndOrbTrigger(choiceContext);
				}
				finally { choiceContext.PopModel(orb); }
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		foreach (var player in room.CombatState.Players)
			NagatoCardOrbs.Clear(player);
		return Task.CompletedTask;
	}
}
