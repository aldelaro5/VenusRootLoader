using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.VenusInternals
{
    internal interface ILeavesRegistry<T, U>
        where T : ILeaf<U>
    {
        Dictionary<string, T> Items { get; }
        T RegisterAndBindNewItem(string namedId, string creatorId);
        T RegisterAndBindExistingItem(U gameId, string namedId, string creatorId);
        T RequestExistingItem(string namedId);
    }
}