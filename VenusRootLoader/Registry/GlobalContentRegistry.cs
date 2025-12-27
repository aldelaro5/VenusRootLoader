using VenusRootLoader.GameContent;

namespace VenusRootLoader.Registry;

internal sealed class GlobalContentRegistry
{
    internal Dictionary<string, (string BudId, ItemContent Content)> Items { get; } = new();
}