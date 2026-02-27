using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface ILeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    IDictionary<string, TLeaf> LeavesByNamedIds { get; }
    IDictionary<int, TLeaf> LeavesByGameIds { get; }
    TLeaf RegisterNew(string namedId, string creatorId);
    TLeaf RegisterExisting(int gameId, string namedId, string creatorId);
    TLeaf Get(string namedId);
    IReadOnlyCollection<TLeaf> GetAll();
}