using System.Buffers.Binary;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Combat;
using STS2RitsuLib.Networking.Sidecar;
using STS2RitsuLib.Utils;

namespace NagatoSpire2.NagatoSpire2Code.Networking;

internal static class NagatoFleetVoteSync
{
	internal readonly record struct Vote(ulong Owner, uint ChoiceId, uint Revision, int Option);
	private sealed class State
	{
		public readonly Dictionary<(ulong, uint), NagatoFleetBallot> Active = [];
		public readonly Dictionary<(ulong, uint), Dictionary<ulong, Vote>> Pending = [];
		public readonly Dictionary<ulong, uint> Closed = [];
	}

	private static readonly AttachedState<PlayerChoiceSynchronizer, State> States = new(() => new State());
	internal static readonly RitsuLibSidecarSyncMessageDescriptor<Vote> Descriptor = new(
		MainFile.ModId, "fleet_vote_revision_v1", Serialize, Deserialize, Receive,
		LocationTargeted: true, ShouldBroadcast: true);

	public static void Register() => RitsuLibSidecarSyncMessages.Register(Descriptor);

	public static NagatoFleetBallot Begin(ulong owner, uint choiceId, IReadOnlyList<ulong> players, int optionCount)
	{
		var state = States.GetOrCreate(RunManager.Instance.PlayerChoiceSynchronizer);
		var key = (owner, choiceId);
		var ballot = new NagatoFleetBallot(players, optionCount);
		state.Active.Add(key, ballot);
		if (state.Pending.Remove(key, out var pending))
			foreach (var (player, vote) in pending)
				ballot.Accept(player, vote.Option, vote.Revision);
		return ballot;
	}

	public static bool Send(ulong owner, uint choiceId, ulong player, uint revision, int option)
	{
		var run = RunManager.Instance;
		var vote = new Vote(owner, choiceId, revision, option);
		if (!RitsuLibSidecarSyncMessages.Send(run.NetService, Descriptor, vote))
			return false;
		Accept(States.GetOrCreate(run.PlayerChoiceSynchronizer), player, vote);
		return true;
	}

	public static void End(ulong owner, uint choiceId)
	{
		var state = States.GetOrCreate(RunManager.Instance.PlayerChoiceSynchronizer);
		if (state.Active.Remove((owner, choiceId), out var ballot))
			ballot.Lock();
		state.Closed[owner] = choiceId;
		foreach (var key in state.Pending.Keys.Where(key => key.Item1 == owner && key.Item2 <= choiceId).ToArray())
			state.Pending.Remove(key);
	}

	private static Task Receive(RitsuLibSidecarSyncMessageContext<Vote> context)
	{
		var run = RunManager.Instance;
		if (ReferenceEquals(context.NetService, run.NetService))
			Accept(States.GetOrCreate(run.PlayerChoiceSynchronizer), context.SenderNetId, context.Message);
		return Task.CompletedTask;
	}

	private static void Accept(State state, ulong player, Vote vote)
	{
		if (vote.Revision == 0 || vote.Option < 0 ||
			state.Closed.TryGetValue(vote.Owner, out uint closed) && vote.ChoiceId <= closed)
			return;
		var key = (vote.Owner, vote.ChoiceId);
		if (state.Active.TryGetValue(key, out var ballot))
		{
			ballot.Accept(player, vote.Option, vote.Revision);
			return;
		}
		if (!state.Pending.TryGetValue(key, out var pending))
			state.Pending[key] = pending = [];
		if (!pending.TryGetValue(player, out var previous) || vote.Revision > previous.Revision)
			pending[player] = vote;
	}

	internal static byte[] Serialize(Vote vote)
	{
		byte[] data = new byte[20];
		BinaryPrimitives.WriteUInt64LittleEndian(data, vote.Owner);
		BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(8), vote.ChoiceId);
		BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(12), vote.Revision);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16), vote.Option);
		return data;
	}

	internal static Vote Deserialize(ReadOnlySpan<byte> data)
	{
		if (data.Length != 20)
			throw new InvalidDataException("Invalid fleet vote message length.");
		return new Vote(BinaryPrimitives.ReadUInt64LittleEndian(data),
			BinaryPrimitives.ReadUInt32LittleEndian(data[8..]),
			BinaryPrimitives.ReadUInt32LittleEndian(data[12..]),
			BinaryPrimitives.ReadInt32LittleEndian(data[16..]));
	}
}
