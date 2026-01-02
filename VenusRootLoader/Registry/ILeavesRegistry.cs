using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface ILeavesRegistry<TLeaf, TGameId>
    where TLeaf : ILeaf<TGameId>
{
    IDictionary<string, TLeaf> Leaves { get; }
    TLeaf RegisterNew(string namedId, string creatorId);
    TLeaf RegisterExisting(TGameId gameId, string namedId, string creatorId);
    TLeaf Get(string namedId);
}