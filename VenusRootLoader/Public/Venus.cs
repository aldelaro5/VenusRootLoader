using CommunityToolkit.Diagnostics;
using VenusRootLoader.GameContent;
using VenusRootLoader.Public.Leaves;
using VenusRootLoader.VenusInternals;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Public;

public sealed class Venus
{
    private readonly VenusServices _venusServices;
    private readonly string _budId;
    private bool _hasGlobalBehaviour;

    internal Venus(string budId, VenusServices venusServices)
    {
        _budId = budId;
        _venusServices = venusServices;
    }

    public GlobalMonoBehaviour SetGlobalMonoBehaviour<T>()
        where T : GlobalMonoBehaviour
    {
        if (_hasGlobalBehaviour)
            throw new InvalidOperationException($"Cannot call {nameof(SetGlobalMonoBehaviour)} more than once");

        GlobalMonoBehaviour globalMonoBehavior =
            _venusServices.GlobalMonoBehaviourExecution.AddGlobalMonoBehavior<T>(_budId);
        _hasGlobalBehaviour = true;
        return globalMonoBehavior;
    }

    public GlobalMonoBehaviour? GetGlobalMonoBehaviourFromBud(string budId)
    {
        Guard.IsNotNullOrWhiteSpace(budId);
        return _venusServices.GlobalMonoBehaviourExecution.GetGlobalMonoBehaviourFromGameObject(budId);
    }

    public ItemLeaf RegisterItem(string namedId)
    {
        if (_venusServices.GlobalContentRegistry.Items.ContainsKey(namedId))
            throw new Exception($"Item with namedId {namedId} already exists");

        ItemContent itemContent = _venusServices.ContentBinder.Items.BindNew(namedId, _budId);
        _venusServices.GlobalContentRegistry.Items[namedId] = itemContent;
        return new ItemLeaf(itemContent, _venusServices.Logger) { OwnerId = _budId };
    }

    public ItemLeaf RequestItem(string namedId)
    {
        return !_venusServices.GlobalContentRegistry.Items.TryGetValue(namedId, out ItemContent content)
            ? throw new Exception($"Item with namedId {namedId} does not exist")
            : new ItemLeaf(content, _venusServices.Logger) { OwnerId = _budId };
    }
}