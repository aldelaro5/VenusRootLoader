using CommunityToolkit.Diagnostics;
using System.Collections;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class LeafList<TLeaf> : ILeafList<TLeaf>
    where TLeaf : Leaf
{
    private readonly List<TLeaf> _list = [];
    private readonly HashSet<string> _namedIds = [];

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public TLeaf this[int index]
    {
        get => _list[index];
        set
        {
            if (_list[index].EffectiveId != value.EffectiveId && _namedIds.Contains(value.EffectiveId))
                ThrowHelper.ThrowInvalidOperationException(FormatLeafAlreadyExistsExceptionMessage(value));

            _namedIds.Remove(_list[index].EffectiveId);
            _namedIds.Add(value.EffectiveId);
            _list[index] = value;
        }
    }

    public IEnumerator<TLeaf> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    public void CopyTo(TLeaf[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public void Add(TLeaf item)
    {
        if (_namedIds.Contains(item.EffectiveId))
            ThrowHelper.ThrowInvalidOperationException(FormatLeafAlreadyExistsExceptionMessage(item));
        _namedIds.Add(item.EffectiveId);
        _list.Add(item);
    }

    public void Insert(int index, TLeaf item)
    {
        if (_namedIds.Contains(item.EffectiveId))
            ThrowHelper.ThrowInvalidOperationException(FormatLeafAlreadyExistsExceptionMessage(item));
        _namedIds.Add(item.EffectiveId);
        _list.Insert(index, item);
    }

    public int IndexOf(TLeaf item) => _list.IndexOf(item);
    public bool Contains(TLeaf item) => _list.Contains(item);

    public bool Remove(TLeaf item)
    {
        bool removed = _list.Remove(item);
        if (removed)
            _namedIds.Remove(item.EffectiveId);
        return removed;
    }

    public void RemoveAt(int index)
    {
        _namedIds.Remove(_list[index].EffectiveId);
        _list.RemoveAt(index);
    }

    public void Clear()
    {
        _namedIds.Clear();
        _list.Clear();
    }

    private static string FormatLeafAlreadyExistsExceptionMessage(TLeaf value) =>
        $"There's already a leaf in this list named {value.NamedId} created by {value.CreatorId}";
}