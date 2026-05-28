using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

/// <inheritdoc/>
internal abstract class BaseRegistry<TLeaf> : ILeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    private readonly ILogger _logger;
    private readonly string _registryName = typeof(TLeaf).Name;

    protected BaseRegistry(ILogger logger) => _logger = logger;

    public IDictionary<string, TLeaf> LeavesByNamedIds { get; } = new Dictionary<string, TLeaf>();
    public IDictionary<int, TLeaf> LeavesByGameIds { get; } = new Dictionary<int, TLeaf>();

    protected abstract int CreateNewGameId(string namedId, string creatorId);

    public TLeaf RegisterNew(string namedId, string creatorId)
    {
        return RegisterNew<TLeaf>(namedId, creatorId);
    }

    public TSubLeaf RegisterNew<TSubLeaf>(string namedId, string creatorId) where TSubLeaf : TLeaf
    {
        EnsureNamedIdIsFree(namedId);
        int gameId = CreateNewGameId(namedId, creatorId);
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, namedId, creatorId);
        LeavesByNamedIds[namedId] = leaf;
        LeavesByGameIds[gameId] = leaf;
        LogRegisterContent(leaf);
        return leaf;
    }

    public virtual TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        return RegisterExisting<TLeaf>(gameId, namedId, creatorId);
    }

    public TSubLeaf RegisterExisting<TSubLeaf>(int gameId, string namedId, string creatorId) where TSubLeaf : TLeaf
    {
        TSubLeaf leaf = CreateLeafInstance<TSubLeaf>(gameId, namedId, creatorId);
        LeavesByNamedIds[namedId] = leaf;
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

    public TLeaf Get(string namedId) => EnsureNamedIdExists(namedId);
    public IReadOnlyCollection<TLeaf> GetAll() => LeavesByNamedIds.Values.ToList().AsReadOnly();

    private void EnsureNamedIdIsFree(string namedId)
    {
        if (LeavesByNamedIds.ContainsKey(namedId))
        {
            ThrowHelper.ThrowArgumentException(
                nameof(namedId),
                $"\"{namedId}\" already exists in the {_registryName} registry");
        }
    }

    private TLeaf EnsureNamedIdExists(string namedId)
    {
        if (!LeavesByNamedIds.TryGetValue(namedId, out TLeaf content))
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