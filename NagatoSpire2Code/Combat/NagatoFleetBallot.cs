namespace NagatoSpire2.NagatoSpire2Code.Combat;

internal sealed class NagatoFleetBallot(IReadOnlyList<ulong> players, int optionCount)
{
	private readonly Dictionary<ulong, int> _votes = [];
	private readonly Dictionary<ulong, uint> _revisions = [];
	private readonly TaskCompletionSource<int[]> _completed = new();
	public IReadOnlyDictionary<ulong, int> Votes => _votes;
	public Task<int[]> Completed => _completed.Task;
	public bool IsLocked { get; private set; }
	public event Action? Changed;

	public bool Accept(ulong player, int option, uint revision)
	{
		if (IsLocked || !players.Contains(player) || option < 0 || option >= optionCount || revision == 0 ||
			_revisions.TryGetValue(player, out uint previous) && revision <= previous)
			return false;
		_revisions[player] = revision;
		_votes[player] = option;
		Changed?.Invoke();
		if (_votes.Count == players.Count)
			_completed.TrySetResult(players.Select(id => _votes[id]).ToArray());
		return true;
	}

	public void Lock()
	{
		IsLocked = true;
		_completed.TrySetCanceled();
	}
}
