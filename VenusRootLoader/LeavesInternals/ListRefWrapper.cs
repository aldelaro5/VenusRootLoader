using System.Collections;

namespace VenusRootLoader.LeavesInternals;

/// <summary>
/// An <see cref="IList{T}"/> that wraps a <see cref="List{T}"/> whose type is a <see cref="Ref{T}"/> where T is a value type.
/// This is done through 2 <see cref="List{T}"/>, one that holds the wrapper elements and one that holds the wrapped <see cref="Ref{T}"/> elements.
/// Both lists are constantly synchronized with each other as operations happens on the exposed <see cref="IList{T}"/>.
/// </summary>
/// <remarks>
/// The purpose of this is to expose a type with list semantics through a different type than the underlying list's type with
/// the ability to bind all the properties of the wrapper to the underlying value.
/// This is possible through <see cref="Ref{T}"/> because it allows to refer to a value type by reference which allows the wrapper
/// to directly change its underlying value in the underlying list.
/// </remarks>
/// <typeparam name="TWrapper">The element type to expose that wraps <see cref="Ref{T}"/> of <typeparamref name="TWrapped"/>.</typeparam>
/// <typeparam name="TWrapped">The element's <see cref="Ref{T}"/>'s type of the underlying list.</typeparam>
internal sealed class ListRefWrapper<TWrapper, TWrapped> : IList<TWrapper>
    where TWrapped : struct
{
    private readonly List<Ref<TWrapped>> _wrappedList;
    private readonly int _startingIndex;
    private readonly Func<TWrapper, Ref<TWrapped>> _refWrapper;
    private readonly List<TWrapper> _backingList = new();

    /// <summary>
    /// Creates a new instance that binds the underlying list and a wrapper function.
    /// </summary>
    /// <param name="wrappedList">The underlying list this type will be bound to.</param>
    /// <param name="startingIndex">The starting index to bind the underlying list's elements.</param>
    /// <param name="refWrapper">A function that transforms a <typeparamref name="TWrapper"/> into a <see cref="Ref{T}"/> of <typeparamref name="TWrapped"/>.</param>
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