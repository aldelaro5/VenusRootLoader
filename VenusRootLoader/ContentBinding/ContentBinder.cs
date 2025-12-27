namespace VenusRootLoader.ContentBinding;

internal sealed class ContentBinder
{
    internal ItemBinder Items { get; }

    public ContentBinder(ItemBinder items)
    {
        Items = items;
    }
}