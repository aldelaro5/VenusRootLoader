using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface ILeavesRegistry<TLeaf>
    where TLeaf : ILeaf
{
    IDictionary<string, TLeaf> Leaves { get; }
    TLeaf RegisterNew(string namedId, string creatorId);
    TLeaf RegisterExisting(int gameId, string namedId, string creatorId);
    TLeaf Get(string namedId);
}