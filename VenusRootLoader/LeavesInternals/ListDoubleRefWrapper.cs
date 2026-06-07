using System.Collections;

namespace VenusRootLoader.LeavesInternals;

internal sealed class ListDoubleRefWrapper<TWrapper, TWrapped1, TWrapped2> : IList<TWrapper>
    where TWrapped1 : struct
    where TWrapped2 : struct
{
    private readonly List<Ref<TWrapped1>> _wrappedList1;
    private readonly List<Ref<TWrapped2>> _wrappedList2;
    private readonly int _startingIndex;
    private readonly Func<TWrapper, Ref<TWrapped1>> _refWrapper1;
    private readonly Func<TWrapper, Ref<TWrapped2>> _refWrapper2;
    private readonly List<TWrapper> _backingList = new();

    internal ListDoubleRefWrapper(
        List<Ref<TWrapped1>> wrappedList1,
        List<Ref<TWrapped2>> wrappedList2,
        int startingIndex,
        Func<TWrapper, Ref<TWrapped1>> refWrapper1,
        Func<TWrapper, Ref<TWrapped2>> refWrapper2)
    {
        _wrappedList1 = wrappedList1;
        _wrappedList2 = wrappedList2;
        _startingIndex = startingIndex;
        _refWrapper1 = refWrapper1;
        _refWrapper2 = refWrapper2;
    }

    internal void SynchronizeFromExistingData(List<TWrapper> existingData)
    {
        _wrappedList1.RemoveRange(_startingIndex, existingData.Count);
        _wrappedList2.RemoveRange(_startingIndex, existingData.Count);
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
        set
        {
            _wrappedList1[index + _startingIndex] = _refWrapper1(value);
            _wrappedList2[index + _startingIndex] = _refWrapper2(value);
        }
    }

    public void Add(TWrapper item)
    {
        _backingList.Add(item);
        _wrappedList1.Add(_refWrapper1(item));
        _wrappedList2.Add(_refWrapper2(item));
    }

    public void Insert(int index, TWrapper item)
    {
        _backingList.Insert(index, item);
        _wrappedList1.Insert(index + _startingIndex, _refWrapper1(item));
        _wrappedList2.Insert(index + _startingIndex, _refWrapper2(item));
    }

    public void CopyTo(TWrapper[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);
    public bool Contains(TWrapper item) => _backingList.Contains(item);
    public int IndexOf(TWrapper item) => _backingList.IndexOf(item);

    public bool Remove(TWrapper item)
    {
        TWrapped1 wrapped1 = _refWrapper1(item).Value;
        TWrapped2 wrapped2 = _refWrapper2(item).Value;
        Ref<TWrapped1>? refWrapper1 = _wrappedList1.Skip(_startingIndex).FirstOrDefault(x => x.Value.Equals(wrapped1));
        Ref<TWrapped2>? refWrapper2 = _wrappedList2.Skip(_startingIndex).FirstOrDefault(x => x.Value.Equals(wrapped2));
        if (refWrapper1 is null || refWrapper2 is null)
            return false;

        return _wrappedList1.Remove(refWrapper1) &&
               _wrappedList2.Remove(refWrapper2) &&
               _backingList.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _backingList.RemoveAt(index);
        _wrappedList1.RemoveAt(index + _startingIndex);
        _wrappedList2.RemoveAt(index + _startingIndex);
    }

    public void Clear()
    {
        _wrappedList1.Clear();
        _wrappedList2.Clear();
        _backingList.Clear();
    }
}