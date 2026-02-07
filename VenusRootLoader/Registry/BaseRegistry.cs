using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal abstract class BaseRegistry<TLeaf> : ILeavesRegistry<TLeaf>
    where TLeaf : ILeaf, new()
{
    private readonly ILogger _logger;
    private readonly string _registryName = typeof(TLeaf).Name;

    protected BaseRegistry(ILogger logger) => _logger = logger;

    public IDictionary<string, TLeaf> Leaves { get; } = new Dictionary<string, TLeaf>();

    protected abstract int CreateNewGameId(string namedId, string creatorId);

    public TLeaf RegisterNew(string namedId, string creatorId)
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
        LogRegisterContent(leaf);
        return leaf;
    }

    public TLeaf Get(string namedId) => EnsureNamedIdExists(namedId);
    public IReadOnlyCollection<TLeaf> GetAll() => Leaves.Values.ToList().AsReadOnly();

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