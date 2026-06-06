using System.Collections;

namespace VenusRootLoader.LeavesInternals;

internal sealed class ListWrapper<TWrapper, TWrapped> : IList<TWrapper>
    where TWrapper : IHasUnderluingValue<TWrapped>
    where TWrapped : struct
{
    private readonly List<TWrapped> _wrappedList;
    private readonly List<TWrapper> _backingList = new();

    internal ListWrapper(List<TWrapped> wrappedList)
    {
        _wrappedList = wrappedList;
    }

    internal void SynchronizeFromExistingData(List<TWrapper> existingData)
    {
        _backingList.AddRange(existingData);
    }

    public int Count => _backingList.Count;
    public bool IsReadOnly => false;

    public IEnumerator<TWrapper> GetEnumerator() => _backingList.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TWrapper this[int index]
    {
        get => _backingList[index];
        set => _wrappedList[index] = value.UnderlyingRef;
    }

    public void Add(TWrapper item)
    {
        _backingList.Add(item);
        _wrappedList.Add(item.UnderlyingRef);
    }

    public void Insert(int index, TWrapper item)
    {
        _wrappedList.Insert(index, item.UnderlyingRef);
        _backingList.Insert(index, item);
    }

    public void CopyTo(TWrapper[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);
    public bool Contains(TWrapper item) => _backingList.Contains(item);
    public int IndexOf(TWrapper item) => _backingList.IndexOf(item);

    public bool Remove(TWrapper item)
    {
        _backingList.Remove(item);
        return _wrappedList.Remove(item.UnderlyingRef);
    }

    public void RemoveAt(int index)
    {
        _wrappedList.RemoveAt(index);
        _backingList.RemoveAt(index);
    }

    public void Clear()
    {
        _wrappedList.Clear();
        _backingList.Clear();
    }
}