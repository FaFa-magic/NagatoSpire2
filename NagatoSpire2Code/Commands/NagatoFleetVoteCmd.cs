using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Cards;
using NagatoSpire2.NagatoSpire2Code.Nodes.Screens;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoFleetVoteCmd
{
	public static async Task Choose(PlayerChoiceContext choiceContext, CardModel source, IReadOnlyList<NagatoFleetChoiceOption> options)
	{
		var combatState = source.CombatState;
		var players = source.Owner.RunState.Players
			.Where(player => player.PlayerCombatState != null && !player.Creature.IsDead && player.Creature.CombatState == combatState)
			.ToArray();
		if (players.Length == 0 || options.Count == 0 || CombatManager.Instance.IsOverOrEnding)
			return;
		int[] votes;
		if (CardSelectCmd.Selector is { } selector)
		{
			votes = new int[players.Length];
			for (int i = 0; i < players.Length; i++)
			{
				var selected = (await selector.GetSelectedCards(options, 1, 1)).Single();
				votes[i] = options.ToList().IndexOf((NagatoFleetChoiceOption)selected);
			}
		}
		else
		{
			var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
			var ids = players.ToDictionary(player => player.NetId, synchronizer.ReserveChoiceId);
			var displayedVotes = new Dictionary<ulong, int>();
			NNagatoFleetVoteScreen? screen = null;
			void ReceiveVote(Player player, uint id, NetPlayerChoiceResult result)
			{
				if (!ids.TryGetValue(player.NetId, out var expected) || id != expected ||
					result.indexes is not { Count: 1 } indexes || indexes[0] < 0 || indexes[0] >= options.Count)
					return;
				displayedVotes[player.NetId] = indexes[0];
				screen?.RefreshVotes();
			}
			synchronizer.PlayerChoiceReceived += ReceiveVote;
			try
			{
				await choiceContext.SignalPlayerChoiceBegun(source.Owner, PlayerChoiceOptions.None);
				var local = players.FirstOrDefault(LocalContext.IsMe);
				bool replay = RunManager.Instance.NetService.Type == NetGameType.Replay;
				if (local != null && !replay)
					screen = NNagatoFleetVoteScreen.Show(options, players, local, displayedVotes);
				async Task<int> Vote(Player player)
				{
					PlayerChoiceResult result;
					if (player == local && !replay)
					{
						int index = await screen!.VoteSubmitted;
						result = PlayerChoiceResult.FromIndex(index);
						synchronizer.SyncLocalChoice(player, ids[player.NetId], result);
					}
					else
						result = await synchronizer.WaitForRemoteChoice(player, ids[player.NetId]);
					int vote = result.AsIndex();
					if (vote < 0 || vote >= options.Count)
						throw new InvalidOperationException("Fleet vote selected an option outside its ballot.");
					displayedVotes[player.NetId] = vote;
					screen?.RefreshVotes();
					return vote;
				}
				votes = await Task.WhenAll(players.Select(Vote));
				await choiceContext.SignalPlayerChoiceEnded();
			}
			finally
			{
				synchronizer.PlayerChoiceReceived -= ReceiveVote;
				if (screen != null)
					NOverlayStack.Instance?.Remove(screen);
			}
		}
		if (CombatManager.Instance.IsOverOrEnding || source.Owner.Creature.IsDead || source.CombatState != combatState)
			return;
		int[] winners = GetWinningOptions(votes, options.Count);
		int winner = winners.Length == 1 ? winners[0] : source.Owner.RunState.Rng.CombatCardSelection.NextItem(winners);
		foreach (var player in players)
			if (player.PlayerCombatState != null && !player.Creature.IsDead && player.Creature.CombatState == combatState)
				await NagatoChoiceCmd.Resolve(choiceContext, player, () => options[winner].ApplyTo(choiceContext, player, source));
	}

	internal static int[] GetWinningOptions(IEnumerable<int> votes, int optionCount)
	{
		int[] counts = new int[optionCount];
		foreach (int vote in votes)
		{
			if (vote < 0 || vote >= optionCount)
				throw new InvalidOperationException("Invalid fleet vote index.");
			counts[vote]++;
		}
		int highest = counts.Max();
		return Enumerable.Range(0, optionCount).Where(index => counts[index] == highest).ToArray();
	}
}
