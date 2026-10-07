using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Nodes;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoCardOrderCmd
{
	public static async Task ReorderDrawPile(PlayerChoiceContext choiceContext, Player player, LocString prompt)
	{
		if (player.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
			return;
		CardPile draw = state.DrawPile;
		CardModel[] original = draw.Cards.ToArray();
		if (original.Length < 2)
			return;

		CardModel[] ordered = await SelectOrder(choiceContext, player, original, prompt);
		if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state)
			return;

		ordered = NagatoCardOrder.RetainCurrent(draw.Cards, ordered);
		if (!draw.Cards.SequenceEqual(ordered))
			await CardPileCmd.Add(ordered.Reverse(), draw, CardPilePosition.Top, skipVisuals: true);
	}

	private static async Task<CardModel[]> SelectOrder(PlayerChoiceContext choiceContext, Player player, CardModel[] original, LocString prompt)
	{
		if (CardSelectCmd.Selector is { } selector)
			return NagatoCardOrder.Complete(original, await selector.GetSelectedCards(original, original.Length, original.Length));

		if (CombatManager.Instance.IsPlayerReadyToEndTurn(player) && player.Creature.CombatState?.CurrentSide == CombatSide.Player)
			CombatManager.Instance.UndoReadyToEndTurn(player);
		var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
		uint choiceId = synchronizer.ReserveChoiceId(player);
		await choiceContext.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);
		try
		{
			if (LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				CardModel[] ordered;
				if (CardSelectCmd.LocalSelector is { } localSelector)
					ordered = NagatoCardOrder.Complete(original, await localSelector.GetSelectedCards(original, original.Length, original.Length));
				else
				{
					var overlays = NOverlayStack.Instance ?? throw new InvalidOperationException("Card ordering requires the combat overlay stack.");
					NPlayerHand.Instance?.CancelAllCardPlay();
					var screen = NNagatoCardOrderScreen.Create(original, prompt);
					overlays.Push(screen);
					ordered = await screen.CardsOrdered();
				}
				synchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableCombatCards(ordered));
				return ordered;
			}

			CardModel[] remote = (await synchronizer.WaitForRemoteChoice(player, choiceId)).AsCombatCards().ToArray();
			if (remote.Length != original.Length)
				throw new InvalidOperationException("Synchronized card order must include every draw-pile card.");
			return NagatoCardOrder.Complete(original, remote);
		}
		finally
		{
			await choiceContext.SignalPlayerChoiceEnded();
		}
	}
}
