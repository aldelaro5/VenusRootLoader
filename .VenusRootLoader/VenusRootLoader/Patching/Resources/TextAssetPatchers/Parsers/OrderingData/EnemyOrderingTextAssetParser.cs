using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.OrderingData;

/// <inheritdoc/>
internal sealed class EnemyOrderingTextAssetParser : IOrderingTextAssetParser<HasEnemyLeaf>
{
    public string GetTextAssetString(IOrderedLeavesRegistry<HasEnemyLeaf> orderedRegistry)
    {
        IReadOnlyCollection<HasEnemyLeaf> orderedLeaves = orderedRegistry.GetOrderedLeaves();
        return string.Join("\n", orderedLeaves.Select(l => l.GameId));
    }

    public void FromTextAssetString(string text, IOrderedLeavesRegistry<HasEnemyLeaf> orderedRegistry)
    {
        int[] orderedGameIds = text
            .Split('\n')
            .Select(line => int.Parse(line, CultureInfo.InvariantCulture))
            .ToArray();
        orderedRegistry.SetBaseGameOrdering(orderedGameIds);
    }
}