using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class OrderedLeavesRegistry<TLeaf> : IOrderedLeavesRegistry<TLeaf>
    where TLeaf : ILeaf, new()
{
    private class LeafOrdering
    {
        internal required TLeaf Leaf { get; init; }
        internal required int? AfterBaseGameId { get; init; }
        internal required int Priority { get; init; }
    }

    private int _nextCreatorIdPriority;

    private Dictionary<int, int> BaseGameIdsToOrderingIndex { get; } = new();
    private List<LeafOrdering> LeavesOrderingData { get; } = new();
    private Dictionary<string, int> CreatorOrderingPriorities { get; } = new();

    public ILeavesRegistry<TLeaf> Registry { get; }

    public OrderedLeavesRegistry(ILeavesRegistry<TLeaf> registry)
    {
        Registry = registry;
    }

    public TLeaf RegisterNewWithOrdering(string namedId, string creatorId, int? orderAfterBaseGameId, int orderPriority)
    {
        TLeaf leaf = Registry.RegisterNew(namedId, creatorId);
        LeavesOrderingData.Add(
            new()
            {
                Leaf = leaf,
                AfterBaseGameId = orderAfterBaseGameId,
                Priority = orderPriority
            });
        if (CreatorOrderingPriorities.ContainsKey(creatorId))
            return leaf;

        CreatorOrderingPriorities.Add(creatorId, _nextCreatorIdPriority);
        _nextCreatorIdPriority++;
        return leaf;
    }

    public TLeaf RegisterExistingWithOrdering(int gameId, string namedId, string creatorId)
    {
        TLeaf leaf = Registry.RegisterExisting(gameId, namedId, creatorId);
        LeavesOrderingData.Add(
            new()
            {
                Leaf = leaf,
                AfterBaseGameId = leaf.GameId,
                Priority = int.MinValue
            });
        return leaf;
    }

    public void SetBaseGameOrdering(int[] orderedGameIds)
    {
        BaseGameIdsToOrderingIndex.Clear();
        for (int i = 0; i < orderedGameIds.Length; i++)
            BaseGameIdsToOrderingIndex.Add(orderedGameIds[i], i);
    }

    public IReadOnlyCollection<TLeaf> GetOrderedLeaves()
    {
        List<TLeaf> leaves = LeavesOrderingData
            .GroupBy(lod => lod.AfterBaseGameId)
            .OrderBy(baseGameGroup => baseGameGroup.Key is null
                ? -1
                : BaseGameIdsToOrderingIndex[baseGameGroup.Key.Value])
            .SelectMany(baseGameGroup => baseGameGroup
                .OrderBy(lod => lod.Leaf.CreatorId == Constants.BaseGameId
                    ? -1
                    : CreatorOrderingPriorities[lod.Leaf.CreatorId])
                .ThenBy(lod => lod.Priority)
                .Select(lod => lod.Leaf)
                .ToList())
            .ToList();
        return leaves.AsReadOnly();
    }
}