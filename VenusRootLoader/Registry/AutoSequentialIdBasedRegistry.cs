using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal enum IdSequenceDirection
{
    Increment,
    Decrement
}

/// <summary>
/// A registry whose <see cref="Leaf"/>'s <see cref="Leaf.GameId"/> changes in a sequential manner from a starting point
/// (ascending or descending) as leaves gets registered to it.
/// </summary>
/// <typeparam name="TLeaf"><inheritdoc/></typeparam>
internal sealed class AutoSequentialIdBasedRegistry<TLeaf> : BaseRegistry<TLeaf>
    where TLeaf : Leaf
{
    private readonly IdSequenceDirection _idSequenceDirection;
    private int _nextAutoIncrementId;

    public AutoSequentialIdBasedRegistry(
        ILogger logger,
        IdSequenceDirection idSequenceDirection,
        int firstGameId = 0) : base(logger)
    {
        _idSequenceDirection = idSequenceDirection;
        _nextAutoIncrementId = firstGameId;
    }

    protected override int CreateNewGameId(string namedId, string creatorId)
    {
        int newGameId = _nextAutoIncrementId;
        _nextAutoIncrementId = _idSequenceDirection switch
        {
            IdSequenceDirection.Increment => _nextAutoIncrementId + 1,
            IdSequenceDirection.Decrement => _nextAutoIncrementId - 1,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>(nameof(_idSequenceDirection))
        };
        return newGameId;
    }

    public override TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        TLeaf leaf = base.RegisterExisting(gameId, namedId, creatorId);
        _nextAutoIncrementId = _idSequenceDirection switch
        {
            IdSequenceDirection.Increment => gameId + 1,
            IdSequenceDirection.Decrement => gameId - 1,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>(nameof(_idSequenceDirection))
        };
        return leaf;
    }
}