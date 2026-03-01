using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.OrderingData;

internal sealed class MedalOrderingTextAssetParser : IOrderingTextAssetParser<MedalLeaf>
{
    public string GetTextAssetString(IOrderedLeavesRegistry<MedalLeaf> orderedRegistry)
    {
        IReadOnlyCollection<MedalLeaf> orderedLeaves = orderedRegistry.GetOrderedLeaves();
        return string.Join("\n", orderedLeaves.Select(l => l.GameId));
    }

    public void FromTextAssetString(string text, IOrderedLeavesRegistry<MedalLeaf> orderedRegistry)
    {
        int[] orderedGameIds = text
            .Split('\n')
            .Select(line => int.Parse(line, CultureInfo.InvariantCulture))
            .ToArray();
        orderedRegistry.SetBaseGameOrdering(orderedGameIds);
    }
}