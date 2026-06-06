using System.Collections;

namespace VenusRootLoader.LeavesInternals;

internal sealed class ListRefWrapper<TWrapper, TWrapped> : IList<TWrapper>
    where TWrapped : struct
{
    private readonly List<Ref<TWrapped>> _wrappedList;
    private readonly int _startingIndex;
    private readonly Func<TWrapper, Ref<TWrapped>> _refWrapper;
    private readonly List<TWrapper> _backingList = new();

    internal ListRefWrapper(
        List<Ref<TWrapped>> wrappedList,
        int startingIndex,
        Func<TWrapper, Ref<TWrapped>> refWrapper)
    {
        _wrappedList = wrappedList;
        _startingIndex = startingIndex;
        _refWrapper = refWrapper;
    }

    internal void SynchronizeFromExistingData(List<TWrapper> existingData)
    {
        _wrappedList.RemoveRange(_startingIndex, existingData.Count);
        _backingList.Clear();
        foreach (TWrapper item in existingData)
            Add(item);
    }

    public int Count => _backingList.Count;
    public bool IsReadOnly => false;

    public IEnumerator<TWrapper> GetEnumerator() => _backingList.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TWrapper this[int index]
    {
        get => _backingList[index];
        set => _wrappedList[index + _startingIndex] = _refWrapper(value);
    }

    public void Add(TWrapper item)
    {
        _backingList.Add(item);
        _wrappedList.Add(_refWrapper(item));
    }

    public void Insert(int index, TWrapper item)
    {
        _backingList.Insert(index, item);
        _wrappedList.Insert(index + _startingIndex, _refWrapper(item));
    }

    public void CopyTo(TWrapper[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);
    public bool Contains(TWrapper item) => _backingList.Contains(item);
    public int IndexOf(TWrapper item) => _backingList.IndexOf(item);
    public bool Remove(TWrapper item)
    {
        TWrapped wrapped = _refWrapper(item).Value;
        Ref<TWrapped>? refWrapper = _wrappedList.Skip(_startingIndex).FirstOrDefault(x => x.Value.Equals(wrapped));
        if (refWrapper is null)
            return false;

        return _wrappedList.Remove(refWrapper) && _backingList.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _backingList.RemoveAt(index);
        _wrappedList.RemoveAt(index + _startingIndex);
    }

    public void Clear()
    {
        _wrappedList.Clear();
        _backingList.Clear();
    }
}