using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

[RegisterSingleton]
public sealed class NagatoCardOrbHooks() : HookedSingletonModel(HookType.Combat)
{
	public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal) =>
		NagatoCardOrbs.Evoke(choiceContext, card);

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy) =>
		card.Capability<CardOrbCapability>()?.UpdateHandFocus() ?? Task.CompletedTask;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		foreach (var card in player.PlayerCombatState?.AllCards.ToArray() ?? [])
			if (card.Capability<CardOrbCapability>() is { } capability)
				await capability.PlayPendingShadow(choiceContext, player);
	}

	public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) =>
		Trigger(choiceContext, player, true);

	public override Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants) => TriggerEnd(choiceContext, side, participants, true);

	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants) => TriggerEnd(choiceContext, side, participants, false);

	private static async Task TriggerEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants, bool mountainPhase)
	{
		if (side != CombatSide.Player)
			return;
		foreach (var creature in participants)
			if (creature.Player is { } player)
				await Trigger(choiceContext, player, false, mountainPhase);
	}

	private static async Task Trigger(PlayerChoiceContext choiceContext, Player player, bool turnStart, bool mountainPhase = false)
	{
		var combat = player.PlayerCombatState;
		foreach (var (card, orb) in NagatoCardOrbs.GetHand(player))
		{
			if (!turnStart && (orb is MountainOrb) != mountainPhase)
				continue;
			if (CombatManager.Instance.IsOverOrEnding || !player.IsActiveForHooks || player.PlayerCombatState != combat)
				break;
			if (card.Pile == combat?.Hand && NagatoCardOrbs.Get(card).Contains(orb))
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

	public override async Task AfterCombatEnd(CombatRoom room)
	{
		foreach (var player in room.CombatState.Players)
			await NagatoCardOrbs.Clear(player);
	}
}
