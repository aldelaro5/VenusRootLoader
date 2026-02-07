using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.TextAssetParsers;

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
            .Select(int.Parse)
            .ToArray();
        registry.SetBaseGameOrdering(orderedGameIds);
    }
}