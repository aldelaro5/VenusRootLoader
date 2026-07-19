using System.Collections;

namespace VenusRootLoader.Api;

internal sealed class ReadOnlyListWithCreate<T> : IReadOnlyList<T>
    where T : IIdentifiable
{
    internal List<T> UnderlyingList { get; } = new();

    public int Count => UnderlyingList.Count;
    public T this[int index] => UnderlyingList[index];
    public IEnumerator<T> GetEnumerator() => UnderlyingList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal T CreateNew(Func<int, T> createNew)
    {
        T item = createNew(Count);
        UnderlyingList.Add(item);
        return item;
    }
}