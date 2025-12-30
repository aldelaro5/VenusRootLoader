using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.LeafBinding;

internal interface ILeafBinder<T, U>
    where T : ILeaf<U>
{
    T BindNew(string namedId, string creatorId);
    T BindExisting(U itemId, string namedId, string creatorId);
}