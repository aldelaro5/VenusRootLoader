using System.Collections;

namespace VenusRootLoader.Api;

public class ReadOnlyListWithCreate<T> : IReadOnlyList<T>
    where T : IIdentifiable, new()
{
    internal List<T> UnderlyingList { get; } = new();

    public int Count => UnderlyingList.Count;
    public T this[int index] => UnderlyingList[index];
    public IEnumerator<T> GetEnumerator() => UnderlyingList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T CreateNew()
    {
        T item = new() { Id = Count };
        UnderlyingList.Add(item);
        return item;
    }
}