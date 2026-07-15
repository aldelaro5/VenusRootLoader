using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Registry;

/// <inheritdoc/>
internal abstract class BaseRegistry<TLeaf> : ILeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    private readonly ILogger _logger;
    private readonly string _registryName = typeof(TLeaf).Name;

    protected BaseRegistry(ILogger logger) => _logger = logger;

    public IDictionary<string, TLeaf> LeavesByEffectiveIds { get; } = new Dictionary<string, TLeaf>();
    public IDictionary<int, TLeaf> LeavesByGameIds { get; } = new Dictionary<int, TLeaf>();

    protected abstract int CreateNewGameId(string effectiveId);

    public TLeaf RegisterNew(string creatorId, string namedId)
    {
        return RegisterNew<TLeaf>(creatorId, namedId);
    }

    public TSubLeaf RegisterNew<TSubLeaf>(string creatorId, string namedId) where TSubLeaf : TLeaf
    {
        EffectiveLeafId.EnsureIdPartIsValid(creatorId, nameof(Leaf.CreatorId));
        EffectiveLeafId.EnsureIdPartIsValid(namedId, nameof(Leaf.NamedId));

        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(namedId);
        if (LeavesByEffectiveIds.ContainsKey(effectiveId))
        {
            ThrowHelper.ThrowArgumentException(
                $"The creator {creatorId} already created a leaf named {namedId} in the {_registryName} registry");
        }

        int gameId = CreateNewGameId(effectiveId);
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, namedId, creatorId);
        LeavesByEffectiveIds[effectiveId] = leaf;
        LeavesByGameIds[gameId] = leaf;
        LogRegisterContent(leaf);
        return leaf;
    }

    public TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        return RegisterExisting<TLeaf>(gameId, namedId, creatorId);
    }

    public virtual TSubLeaf RegisterExisting<TSubLeaf>(int gameId, string namedId, string creatorId)
        where TSubLeaf : TLeaf
    {
        EffectiveLeafId.EnsureIdPartIsValid(creatorId, nameof(Leaf.CreatorId));
        EffectiveLeafId.EnsureIdPartIsValid(namedId, nameof(Leaf.NamedId));

        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, namedId, creatorId);
        LeavesByEffectiveIds[effectiveId] = leaf;
        LeavesByGameIds[gameId] = leaf;
        LogRegisterContent(leaf);
        return leaf;
    }

    private static TSubLeaf CreateLeafInstance<TSubLeaf>(int gameId, string namedId, string creatorId)
        where TSubLeaf : TLeaf
    {
        // We have to use the Activator here because it's not possible to use a generics constraint that does what we want.
        // The closest is new(), but this requires the constructor to be public which we don't want on any leaves since
        // the registry should be the only one allowed to create new leaves from buds
        return (TSubLeaf)Activator.CreateInstance(
            typeof(TSubLeaf),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [
                gameId,
                namedId,
                creatorId
            ],
            null,
            null);
    }

    public TLeaf Get(string creatorId, string namedId)
    {
        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        return GetByEffectiveIdWithThrow(effectiveId);
    }

    public IReadOnlyCollection<TLeaf> GetAll() => LeavesByEffectiveIds.Values.ToList().AsReadOnly();

    private TLeaf GetByEffectiveIdWithThrow(string effectiveId)
    {
        if (!LeavesByEffectiveIds.TryGetValue(effectiveId, out TLeaf content))
        {
            return ThrowHelper.ThrowArgumentException<TLeaf>(
                nameof(effectiveId),
                $"\"{effectiveId}\" does not exist in the {_registryName} registry");
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