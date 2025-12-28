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
        EnsureNamedIdIsFree("Item", namedId, Items);
        ItemContent itemContent = _contentBinder.BindNew(namedId, creatorId);
        Items[namedId] = itemContent;
        return itemContent;
    }

    internal ItemContent RequestExistingItem(string namedId) => EnsureNamedIdExists("Item", namedId, Items);

    private static void EnsureNamedIdIsFree<T>(string contentType, string namedId, Dictionary<string, T> registry)
    {
        if (registry.ContainsKey(namedId))
            throw new Exception($"{contentType} with namedId {namedId} already exists");
    }

    private static T EnsureNamedIdExists<T>(string contentType, string namedId, Dictionary<string, T> registry)
    {
        return !registry.TryGetValue(namedId, out T content)
            ? throw new Exception($"{contentType} with namedId {namedId} does not exist")
            : content;
    }
}