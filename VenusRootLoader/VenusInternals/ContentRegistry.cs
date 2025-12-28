using System.Runtime.CompilerServices;
using VenusRootLoader.ContentBinding;
using VenusRootLoader.GameContent;

namespace VenusRootLoader.VenusInternals;

internal sealed class ContentRegistry
{
    private readonly IContentBinder<ItemContent, int> _contentBinder;

    private Dictionary<string, ItemContent> Items { get; } = new();

    public ContentRegistry(IContentBinder<ItemContent, int> contentBinder)
    {
        _contentBinder = contentBinder;
    }

    internal ItemContent RegisterAndBindNewItem(string namedId, string creatorId)
    {
        EnsureNamedIdIsFree(namedId, Items);
        ItemContent itemContent = _contentBinder.BindNew(namedId, creatorId);
        Items[namedId] = itemContent;
        return itemContent;
    }

    internal ItemContent RequestExistingItem(string namedId) => EnsureNamedIdExists(namedId, Items);

    private static void EnsureNamedIdIsFree<T>(
        string namedId,
        Dictionary<string, T> registry,
        [CallerArgumentExpression(nameof(registry))]
        string registryName = "")
    {
        if (registry.ContainsKey(namedId))
            throw new Exception($"{namedId} already exists in the {registryName} registry");
    }

    private static T EnsureNamedIdExists<T>(
        string namedId,
        Dictionary<string, T> registry,
        [CallerArgumentExpression(nameof(registry))]
        string registryName = "")
    {
        return !registry.TryGetValue(namedId, out T content)
            ? throw new Exception($"{namedId} does not exist in the {registryName} registry")
            : content;
    }
}