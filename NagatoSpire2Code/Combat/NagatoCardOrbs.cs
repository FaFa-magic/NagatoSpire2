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
	public static OrbModel? Get(CardModel card) =>
		(ModelCapabilities.TryGet(card, out var capabilities) ? capabilities.Get<CardOrbCapability>() :
			card is IModelCapabilitySource ? card.Capability<CardOrbCapability>() : null)?.Orb;

	public static void Bind(CardModel card, OrbModel orb)
	{
		card.AssertMutable();
		orb.AssertMutable();
		var combat = card.Owner?.PlayerCombatState;
		if (combat?.OrbQueue.Orbs.Contains(orb) == true || orb.HasBeenRemovedFromState)
			throw new InvalidOperationException("The orb must not be queued or removed.");
		if (orb.Owner != null && orb.Owner != card.Owner)
			throw new InvalidOperationException("The orb and card must have the same owner.");
		if (Get(card) == orb)
			return;
		if (Get(card) != null || combat?.AllCards.Any(other => Get(other) == orb) == true)
			throw new InvalidOperationException("The card or orb is already bound.");
		if (card.Owner is { } owner)
			orb.Owner = owner;
		card.GetOrCreateCapability<CardOrbCapability>().SetOrb(orb);
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);
	}

	public static OrbModel? Unbind(CardModel card)
	{
		if (!card.IsMutable || Get(card) is not { } orb)
			return null;
		card.Capability<CardOrbCapability>()!.SetOrb(null);
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);
		return orb;
	}

	public static async Task Evoke(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead || Unbind(card) is not { } orb)
			return;
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

	public static IEnumerable<OrbModel> GetAll(Player player) =>
		player.PlayerCombatState?.AllCards.Select(Get).OfType<OrbModel>().ToArray() ?? [];

	public static (CardModel Card, OrbModel Orb)[] GetHand(Player player) =>
		player.PlayerCombatState?.Hand.Cards.Select(card => (Card: card, Orb: Get(card)))
			.Where(pair => pair.Orb != null).Select(pair => (pair.Card, pair.Orb!)).ToArray() ?? [];

	public static void Clear(Player player)
	{
		foreach (var card in player.PlayerCombatState?.AllCards.ToArray() ?? [])
			Unbind(card)?.RemoveInternal();
	}
}
