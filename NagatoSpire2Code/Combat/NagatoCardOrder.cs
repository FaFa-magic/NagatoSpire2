namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoCardOrder
{
	public static void Move<T>(List<T> cards, int from, int to)
	{
		if (from < 0 || from >= cards.Count || to < 0 || to >= cards.Count)
			throw new ArgumentOutOfRangeException();
		T card = cards[from];
		cards.RemoveAt(from);
		cards.Insert(to, card);
	}

	public static T[] Complete<T>(IReadOnlyList<T> original, IEnumerable<T> ordered) where T : notnull
	{
		var remaining = original.ToHashSet();
		var result = new List<T>(original.Count);
		foreach (T card in ordered)
		{
			if (!remaining.Remove(card))
				throw new InvalidOperationException("Card order contains a duplicate or an unavailable card.");
			result.Add(card);
		}
		result.AddRange(original.Where(remaining.Contains));
		return result.ToArray();
	}

	public static T[] RetainCurrent<T>(IReadOnlyList<T> current, IReadOnlyList<T> ordered) where T : notnull =>
		ordered.Where(current.Contains).Concat(current.Where(card => !ordered.Contains(card))).ToArray();
}
