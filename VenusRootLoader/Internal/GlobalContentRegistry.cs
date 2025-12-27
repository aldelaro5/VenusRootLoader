using VenusRootLoader.GameContent;

namespace VenusRootLoader.Internal;

internal sealed class GlobalContentRegistry
{
    internal Dictionary<string, (string CreatorId, ItemContent Content)> Items { get; } = new();
}