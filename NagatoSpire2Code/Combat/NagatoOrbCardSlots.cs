using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoOrbCardSlots
{
	public static bool IsEnabled(Player player) => NagatoOrbSlotRun.AppliesTo(player) && player.PlayerCombatState?.OrbQueue.Capacity > 0;

	public static CardModel?[] GetCards(Player player)
	{
		if (!IsEnabled(player) || player.PlayerCombatState is not { } state)
			return [];
		var cards = new CardModel?[state.OrbQueue.Capacity];
		for (int i = 0; i < Math.Min(cards.Length, state.DrawPile.Cards.Count); i++)
			cards[i] = state.DrawPile.Cards[i];
		return cards;
	}
}
