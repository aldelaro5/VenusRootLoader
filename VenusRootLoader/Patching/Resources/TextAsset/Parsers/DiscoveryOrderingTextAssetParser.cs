using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class DiscoveryOrderingTextAssetParser : IOrderingTextAssetParser<DiscoveryLeaf>
{
    public string GetTextAssetString(IOrderedLeavesRegistry<DiscoveryLeaf> orderedRegistry)
    {
        IReadOnlyCollection<DiscoveryLeaf> orderedLeaves = orderedRegistry.GetOrderedLeaves();
        return string.Join("\n", orderedLeaves.Select(l => $"{l.GameId},{l.EnemyPortraitsSpriteIndex!.Value}"));
    }

    public void FromTextAssetString(string text, IOrderedLeavesRegistry<DiscoveryLeaf> orderedRegistry)
    {
        string[][] lines = text.Split('\n')
            .Select(l => l
                .Split(Utility.StringUtils.CommaSplitDelimiter))
            .ToArray();

        Dictionary<int, int> linesData = lines
            .Select(l => (GameId: int.Parse(l[0], CultureInfo.InvariantCulture),
                EnemyPortraitIndex: int.Parse(l[1], CultureInfo.InvariantCulture)))
            .ToDictionary(data => data.GameId, data => data.EnemyPortraitIndex);
        for (int i = 0; i < linesData.Count; i++)
            orderedRegistry.Registry.LeavesByGameIds[i].EnemyPortraitsSpriteIndex = linesData[i];
        orderedRegistry.SetBaseGameOrdering(linesData.Keys.ToArray());
    }
}