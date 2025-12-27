using VenusRootLoader.ContentBinding;
using VenusRootLoader.GameContent;

namespace VenusRootLoader.VenusInternals;

internal sealed class ContentBinder
{
    internal IContentBinder<ItemContent, int> Items { get; }

    public ContentBinder(IContentBinder<ItemContent, int> items)
    {
        Items = items;
    }
}