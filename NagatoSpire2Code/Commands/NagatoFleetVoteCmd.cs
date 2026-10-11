using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Cards;
using NagatoSpire2.NagatoSpire2Code.Networking;
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
		int winner;
		if (CardSelectCmd.Selector is { } selector)
		{
			votes = new int[players.Length];
			for (int i = 0; i < players.Length; i++)
			{
				var selected = (await selector.GetSelectedCards(options, 1, 1)).Single();
				votes[i] = options.ToList().IndexOf((NagatoFleetChoiceOption)selected);
			}
			winner = SelectWinner(GetWinningOptions(votes, options.Count), source);
		}
		else
		{
			var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
			uint id = synchronizer.ReserveChoiceId(source.Owner);
			bool replay = RunManager.Instance.NetService.Type == NetGameType.Replay;
			var ballot = replay ? null : NagatoFleetVoteSync.Begin(source.Owner.NetId, id,
				players.Select(player => player.NetId).ToArray(), options.Count);
			NNagatoFleetVoteScreen? screen = null;
			bool begun = false;
			void RefreshVotes() => screen?.RefreshVotes();
			if (ballot != null)
				ballot.Changed += RefreshVotes;
			try
			{
				await choiceContext.SignalPlayerChoiceBegun(source.Owner, PlayerChoiceOptions.None);
				begun = true;
				var local = players.FirstOrDefault(LocalContext.IsMe);
				var failed = new TaskCompletionSource<int[]>();
				uint revision = 0;
				if (local != null && !replay)
					screen = NNagatoFleetVoteScreen.Show(options, players, local, ballot!.Votes, index =>
				{
					if (ballot.IsLocked || ballot.Votes.TryGetValue(local.NetId, out int previous) && previous == index)
						return;
					try
					{
						if (!NagatoFleetVoteSync.Send(source.Owner.NetId, id, local.NetId, ++revision, index))
							throw new InvalidOperationException("Could not synchronize the fleet vote.");
					}
					catch (Exception exception)
					{
						failed.TrySetException(exception);
					}
				});
				async Task<int[]> Finish()
				{
					if (!replay && LocalContext.IsMe(source.Owner))
					{
						var result = await ballot!.Completed;
						ballot.Lock();
						synchronizer.SyncLocalChoice(source.Owner, id, PlayerChoiceResult.FromIndexes(result.ToList()));
						return result;
					}
					return (await synchronizer.WaitForRemoteChoice(source.Owner, id)).AsIndexes().ToArray();
				}
				var finishing = Task.WhenAny(Finish(), failed.Task);
				var finished = screen == null ? await finishing : await finishing.WaitAsync(screen.Lifetime);
				votes = await finished;
				if (votes.Length != players.Length || votes.Any(vote => vote < 0 || vote >= options.Count))
					throw new InvalidOperationException("Invalid finalized fleet ballot.");
				ballot?.Lock();
				int[] candidates = GetWinningOptions(votes, options.Count);
				winner = SelectWinner(candidates, source);
				if (screen != null)
					await screen.AnimateResult(votes, candidates, winner,
						source.Owner.RunState.Rng.Seed + (ulong)source.Owner.RunState.ActFloor).WaitAsync(screen.Lifetime);
			}
			finally
			{
				if (ballot != null)
				{
					ballot.Changed -= RefreshVotes;
					NagatoFleetVoteSync.End(source.Owner.NetId, id);
				}
				screen?.Close();
				if (begun)
					await choiceContext.SignalPlayerChoiceEnded();
			}
		}
		if (CombatManager.Instance.IsOverOrEnding || source.Owner.Creature.IsDead || source.CombatState != combatState)
			return;
		foreach (var player in players)
			if (player.PlayerCombatState != null && !player.Creature.IsDead && player.Creature.CombatState == combatState)
				await NagatoChoiceCmd.Resolve(choiceContext, player, () => options[winner].ApplyTo(choiceContext, player, source));
	}

	private static int SelectWinner(int[] winners, CardModel source)
	{
		return winners.Length == 1 ? winners[0] : source.Owner.RunState.Rng.CombatCardSelection.NextItem(winners);
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
