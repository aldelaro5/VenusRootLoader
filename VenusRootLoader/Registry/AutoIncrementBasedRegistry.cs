using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal abstract class AutoIncrementBasedRegistry<TLeaf> : BaseRegistry<TLeaf>
    where TLeaf : ILeaf, new()
{
    private int _nextAutoIncrementId;

    protected AutoIncrementBasedRegistry(ILogger logger) : base(logger) { }

    protected sealed override int CreateNewGameId(string namedId, string creatorId)
    {
        int newGameId = _nextAutoIncrementId;
        _nextAutoIncrementId++;
        return newGameId;
    }

    public sealed override TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        TLeaf leaf = base.RegisterExisting(gameId, namedId, creatorId);
        _nextAutoIncrementId = gameId + 1;
        return leaf;
    }
}