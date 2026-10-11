namespace NagatoSpire2.NagatoSpire2Code.Combat;

public sealed class NagatoKnownDrawOrder<T> where T : class
{
    private readonly HashSet<T> _known = new(ReferenceEqualityComparer.Instance);

    public uint Version { get; private set; }

    public bool Contains(T card) => _known.Contains(card);

    public void Remember(IEnumerable<T> cards)
    {
        foreach (var card in cards)
            _known.Add(card);
    }

    public void Forget(T card) => _known.Remove(card);

    public T[] GetTop(IReadOnlyList<T> cards) => cards.TakeWhile(_known.Contains).ToArray();

    public void Invalidate()
    {
        _known.Clear();
        Version++;
    }
}
