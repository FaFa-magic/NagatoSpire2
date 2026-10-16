using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoCardOrbs
{
	public static IReadOnlyList<OrbModel> Get(CardModel card) =>
		(ModelCapabilities.TryGet(card, out var capabilities) ? capabilities.Get<CardOrbCapability>() :
			card is IModelCapabilitySource ? card.Capability<CardOrbCapability>() : null)?.Orbs ?? [];

	public static void Bind(CardModel card, OrbModel orb)
	{
		card.AssertMutable();
		orb.AssertMutable();
		var combat = card.Owner?.PlayerCombatState;
		if (combat?.OrbQueue.Orbs.Contains(orb) == true || orb.HasBeenRemovedFromState)
			throw new InvalidOperationException("The orb must not be queued or removed.");
		if (orb.Owner != null && orb.Owner != card.Owner)
			throw new InvalidOperationException("The orb and card must have the same owner.");
		var orbs = Get(card);
		if (orbs.Contains(orb))
			return;
		if (orbs.Count >= CardOrbCapability.MaxOrbs)
			throw new InvalidOperationException($"A card can host at most {CardOrbCapability.MaxOrbs} orbs.");
		if (combat?.AllCards.Any(other => Get(other).Contains(orb)) == true)
			throw new InvalidOperationException("The orb is already bound.");
		if (card.Owner is { } owner)
			orb.Owner = owner;
		card.GetOrCreateCapability<CardOrbCapability>().SetOrbs([.. orbs, orb]);
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);
	}

	public static IReadOnlyList<OrbModel> Unbind(CardModel card)
	{
		if (!card.IsMutable || Get(card) is not { Count: > 0 } orbs)
			return [];
		card.Capability<CardOrbCapability>()!.SetOrbs();
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);
		return orbs;
	}

	public static async Task Evoke(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead)
			return;
		var orbs = Unbind(card);
		try
		{
			foreach (var orb in orbs)
			{
				if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead)
					break;
				choiceContext.PushModel(orb);
				try
				{
					var targets = await orb.Evoke(choiceContext);
					if (card.Owner.Creature.CombatState is { } combat)
						await Hook.AfterOrbEvoked(choiceContext, combat, orb, targets);
				}
				finally
				{
					orb.RemoveInternal();
					choiceContext.PopModel(orb);
				}
			}
		}
		finally
		{
			foreach (var orb in orbs)
				if (!orb.HasBeenRemovedFromState)
					orb.RemoveInternal();
		}
	}

	public static IEnumerable<OrbModel> GetAll(Player player) =>
		player.PlayerCombatState?.AllCards.SelectMany(Get).ToArray() ?? [];

	public static (CardModel Card, OrbModel Orb)[] GetHand(Player player) =>
		player.PlayerCombatState?.Hand.Cards.SelectMany(card => Get(card).Select(orb => (card, orb))).ToArray() ?? [];

	public static void Clear(Player player)
	{
		foreach (var card in player.PlayerCombatState?.AllCards.ToArray() ?? [])
			foreach (var orb in Unbind(card))
				orb.RemoveInternal();
	}
}
