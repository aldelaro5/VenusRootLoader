using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface IOrderedLeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    ILeavesRegistry<TLeaf> Registry { get; }
    TLeaf RegisterNewWithOrdering(string namedId, string creatorId, int? orderAfterBaseGameId, int orderPriority);
    TLeaf RegisterExistingWithOrdering(int gameId, string namedId, string creatorId);
    void SetBaseGameOrdering(int[] orderedGameIds);
    IReadOnlyCollection<TLeaf> GetOrderedLeaves();
}