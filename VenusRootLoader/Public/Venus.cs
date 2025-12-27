using CommunityToolkit.Diagnostics;
using VenusRootLoader.GameContent;
using VenusRootLoader.Internal;
using VenusRootLoader.Public.Leaves;

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

        ItemContent itemContent = _venusServices.ContentBinder.Items.BindNewItem(namedId);
        _venusServices.GlobalContentRegistry.Items[namedId] = (_budId, itemContent);
        return new ItemLeaf(itemContent, namedId, _budId, _budId);
    }

    public ItemLeaf RequestItem(string namedId)
    {
        if (!_venusServices.GlobalContentRegistry.Items.TryGetValue(
                namedId,
                out (string CreatorId, ItemContent Content) content))
        {
            throw new Exception($"Item with namedId {namedId} does not exist");
        }

        return new ItemLeaf(content.Content, namedId, content.CreatorId, _budId);
    }
}