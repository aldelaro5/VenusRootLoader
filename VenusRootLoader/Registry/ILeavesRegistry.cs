using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface ILeavesRegistry<TLeaf>
    where TLeaf : ILeaf
{
    IDictionary<string, TLeaf> Leaves { get; }
    TLeaf RegisterNew(string namedId, string creatorId, int? orderAfterBaseGameId, int orderPriority);
    TLeaf RegisterExisting(int gameId, string namedId, string creatorId);
    TLeaf Get(string namedId);
    IReadOnlyCollection<TLeaf> GetAll();
    void SetBaseGameOrdering(int[] orderedGameIds);
    IReadOnlyCollection<TLeaf> GetOrderedLeaves();
}