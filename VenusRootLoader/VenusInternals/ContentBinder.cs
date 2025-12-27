using VenusRootLoader.ContentBinding;

namespace VenusRootLoader.VenusInternals;

internal sealed class ContentBinder
{
    internal ItemBinder Items { get; }

    public ContentBinder(ItemBinder items)
    {
        Items = items;
    }
}