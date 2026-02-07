using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal abstract class BaseRegistry<TLeaf> : ILeavesRegistry<TLeaf>
    where TLeaf : ILeaf, new()
{
    private class LeafOrdering
    {
        internal required TLeaf Leaf { get; init; }
        internal required int? AfterBaseGameId { get; init; }
        internal required int Priority { get; init; }
    }

    private readonly ILogger _logger;
    private readonly string _registryName = typeof(TLeaf).Name;

    private int _nextCreatorIdPriority;

    protected BaseRegistry(ILogger logger) => _logger = logger;

    public IDictionary<string, TLeaf> Leaves { get; } = new Dictionary<string, TLeaf>();

    private Dictionary<int, int> BaseGameIdsToOrderingIndex { get; } = new();
    private List<LeafOrdering> LeavesOrderingData { get; } = new();
    private Dictionary<string, int> CreatorOrderingPriorities { get; } = new();
    
    protected abstract int CreateNewGameId(string namedId, string creatorId);

    public TLeaf RegisterNew(string namedId, string creatorId, int? orderAfterBaseGameId, int orderPriority)
    {
        EnsureNamedIdIsFree(namedId);
        int gameId = CreateNewGameId(namedId, creatorId);
        TLeaf leaf = new()
        {
            GameId = gameId,
            CreatorId = creatorId,
            NamedId = namedId
        };
        Leaves[namedId] = leaf;
        LeavesOrderingData.Add(
            new()
            {
                Leaf = leaf,
                AfterBaseGameId = orderAfterBaseGameId,
                Priority = orderPriority
            });
        if (!CreatorOrderingPriorities.ContainsKey(creatorId))
        {
            CreatorOrderingPriorities.Add(creatorId, _nextCreatorIdPriority);
            _nextCreatorIdPriority++;
        }
        LogRegisterContent(leaf);
        return leaf;
    }

    public virtual TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        TLeaf leaf = new()
        {
            GameId = gameId,
            NamedId = namedId,
            CreatorId = creatorId
        };
        Leaves[namedId] = leaf;
        LeavesOrderingData.Add(
            new()
            {
                Leaf = leaf,
                AfterBaseGameId = leaf.GameId,
                Priority = int.MinValue
            });
        LogRegisterContent(leaf);
        return leaf;
    }

    public TLeaf Get(string namedId) => EnsureNamedIdExists(namedId);
    public IReadOnlyCollection<TLeaf> GetAll() => Leaves.Values.ToList().AsReadOnly();

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

    private void EnsureNamedIdIsFree(string namedId)
    {
        if (Leaves.ContainsKey(namedId))
        {
            ThrowHelper.ThrowArgumentException(
                nameof(namedId),
                $"\"{namedId}\" already exists in the {_registryName} registry");
        }
    }

    private TLeaf EnsureNamedIdExists(
        string namedId)
    {
        if (!Leaves.TryGetValue(namedId, out TLeaf content))
        {
            return ThrowHelper.ThrowArgumentException<TLeaf>(
                nameof(namedId),
                $"\"{namedId}\" does not exist in the {_registryName} registry");
        }

        return content;
    }

    private void LogRegisterContent(TLeaf leaf)
    {
        _logger.LogTrace(
            "Registered a new leaf named {NamedId} (game id {GameId}) created by {CreatorId}",
            leaf.NamedId,
            leaf.GameId,
            leaf.CreatorId);
    }
}