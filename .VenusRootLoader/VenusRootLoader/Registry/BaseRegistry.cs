using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Registry;

/// <inheritdoc/>
internal abstract class BaseRegistry<TLeaf> : ILeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    private readonly ILogger _logger;

    protected BaseRegistry(ILogger logger) => _logger = logger;

    private IDictionary<int, TLeaf> LeavesByGameIds { get; } = new Dictionary<int, TLeaf>();

    private IDictionary<string, TLeaf> LeavesByEffectiveIds { get; } = new Dictionary<string, TLeaf>();

    public string RegistryName => typeof(TLeaf).Name;

    public int Count => LeavesByEffectiveIds.Count;

    public int CountBaseGame { get; private set; }

    protected abstract int CreateNewGameId(string effectiveId);

    public IEnumerator<TLeaf> GetEnumerator() => LeavesByGameIds.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TLeaf RegisterNew(string creatorId, string namedId) => RegisterNew<TLeaf>(creatorId, namedId);

    public TSubLeaf RegisterNew<TSubLeaf>(string creatorId, string namedId) where TSubLeaf : TLeaf
    {
        EffectiveLeafId.EnsureIdPartIsValid(creatorId, nameof(Leaf.CreatorId));
        EffectiveLeafId.EnsureIdPartIsValid(namedId, nameof(Leaf.NamedId));

        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        if (LeavesByEffectiveIds.ContainsKey(effectiveId))
        {
            ThrowHelper.ThrowArgumentException(
                $"The creator {creatorId} already created a leaf named {namedId} in the {RegistryName} registry");
        }

        int gameId = CreateNewGameId(effectiveId);
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, creatorId, namedId);
        LeavesByEffectiveIds[effectiveId] = leaf;
        LeavesByGameIds[gameId] = leaf;
        LogRegisterContent(leaf);
        return leaf;
    }

    public TLeaf RegisterExisting(int gameId, string namedId) =>
        RegisterExisting<TLeaf>(gameId, namedId);

    public virtual TSubLeaf RegisterExisting<TSubLeaf>(int gameId, string namedId)
        where TSubLeaf : TLeaf
    {
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, Constants.BaseGameCreatorId, namedId);
        LeavesByEffectiveIds[namedId] = leaf;
        LeavesByGameIds[gameId] = leaf;
        LogRegisterContent(leaf);
        CountBaseGame++;
        return leaf;
    }

    private static TSubLeaf CreateLeafInstance<TSubLeaf>(int gameId, string creatorId, string namedId)
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
                creatorId,
                namedId
            ],
            null,
            null);
    }

    public TLeaf Get(string creatorId, string namedId)
    {
        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        return GetByEffectiveId(effectiveId);
    }

    public bool TryGet(string creatorId, string namedId, [NotNullWhen(true)] out TLeaf? leaf)
    {
        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        if (!LeavesByEffectiveIds.TryGetValue(effectiveId, out TLeaf value))
        {
            leaf = null;
            return false;
        }

        leaf = value;
        return true;
    }

    public TLeaf GetByEffectiveId(string effectiveId)
    {
        if (!LeavesByEffectiveIds.TryGetValue(effectiveId, out TLeaf leaf))
        {
            (string CreatorId, string NamedId) parts = EffectiveLeafId.SplitParts(effectiveId);
            return ThrowHelper.ThrowArgumentException<TLeaf>(
                nameof(effectiveId),
                $"No leaf named {parts.NamedId} by {parts.CreatorId} exists in the {RegistryName} registry");
        }

        return leaf;
    }

    public bool TryGetByEffectiveId(string effectiveId, [NotNullWhen(true)] out TLeaf? leaf)
    {
        if (!LeavesByEffectiveIds.TryGetValue(effectiveId, out TLeaf value))
        {
            leaf = null;
            return false;
        }

        leaf = value;
        return true;
    }

    public TLeaf GetByGameId(int gameId)
    {
        if (!LeavesByGameIds.TryGetValue(gameId, out TLeaf leaf))
        {
            return ThrowHelper.ThrowArgumentException<TLeaf>(
                nameof(gameId),
                $"No leaf with game id {gameId} exists in the {RegistryName} registry");
        }

        return leaf;
    }

    public IReadOnlyCollection<TLeaf> GetAll() => LeavesByEffectiveIds.Values.ToList().AsReadOnly();

    private void LogRegisterContent(TLeaf leaf)
    {
        _logger.LogTrace(
            "Registered a new leaf named {NamedId} (game id {GameId}) created by {CreatorId}",
            leaf.NamedId,
            leaf.GameId,
            leaf.CreatorId);
    }
}