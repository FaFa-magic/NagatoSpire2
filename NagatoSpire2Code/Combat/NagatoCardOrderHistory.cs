namespace NagatoSpire2.NagatoSpire2Code.Combat;

/// <summary>Local editing history only. Nothing is committed until the existing confirm flow.</summary>
public sealed class NagatoCardOrderHistory<T>
{
    private readonly Stack<T[]> _undo = new();
    private T[] _original = [];
    public bool CanUndo => _undo.Count > 0;
    public void Initialize(IEnumerable<T> cards) { _original = cards.ToArray(); _undo.Clear(); }
    public bool IsOriginal(IEnumerable<T> cards) => cards.SequenceEqual(_original);
    public void Remember(IEnumerable<T> cards) => _undo.Push(cards.ToArray());
    public T[] Undo() => _undo.Pop();
    public T[] Original => _original.ToArray();
}
