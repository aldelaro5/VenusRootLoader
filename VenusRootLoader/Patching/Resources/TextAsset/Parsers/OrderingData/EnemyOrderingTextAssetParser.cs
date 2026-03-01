using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.OrderingData;

internal sealed class EnemyOrderingTextAssetParser : IOrderingTextAssetParser<EnemyLeaf>
{
    public string GetTextAssetString(IOrderedLeavesRegistry<EnemyLeaf> orderedRegistry)
    {
        IReadOnlyCollection<EnemyLeaf> orderedLeaves = orderedRegistry.GetOrderedLeaves();
        return string.Join("\n", orderedLeaves.Select(l => l.GameId));
    }

    public void FromTextAssetString(string text, IOrderedLeavesRegistry<EnemyLeaf> orderedRegistry)
    {
        int[] orderedGameIds = text
            .Split('\n')
            .Select(line => int.Parse(line, CultureInfo.InvariantCulture))
            .ToArray();
        orderedRegistry.SetBaseGameOrdering(orderedGameIds);
    }
}