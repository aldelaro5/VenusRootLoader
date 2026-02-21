using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class MedalOrderingTextAssetParser : IOrderingTextAssetParser<MedalLeaf>
{
    public string GetTextAssetString(IOrderedLeavesRegistry<MedalLeaf> registry)
    {
        IReadOnlyCollection<MedalLeaf> orderedLeaves = registry.GetOrderedLeaves();
        return string.Join("\n", orderedLeaves.Select(l => l.GameId));
    }

    public void FromTextAssetString(string text, IOrderedLeavesRegistry<MedalLeaf> registry)
    {
        int[] orderedGameIds = text
            .Split('\n')
            .Select(line => int.Parse(line, CultureInfo.InvariantCulture))
            .ToArray();
        registry.SetBaseGameOrdering(orderedGameIds);
    }
}