using VenusRootLoader.GameContent;

namespace VenusRootLoader.VenusInternals;

internal sealed class GlobalContentRegistry
{
    internal Dictionary<string, ItemContent> Items { get; } = new();
}