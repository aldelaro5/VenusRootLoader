using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.LeavesInternals;

internal interface ILeafResolver<TLeaf> : ILeafId
    where TLeaf : Leaf
{
    TLeaf Resolve();
}

internal sealed class ImmediateLeafResolver<TLeaf> : ILeafResolver<TLeaf>
    where TLeaf : Leaf
{
    public string CreatorId => _leaf.CreatorId;
    public string NamedId => _leaf.NamedId;
    private readonly TLeaf _leaf;

    public ImmediateLeafResolver(TLeaf leaf) => _leaf = leaf;
    public TLeaf Resolve() => _leaf;
}

internal sealed class DeferedLeafResolver<TLeaf> : ILeafResolver<TLeaf>
    where TLeaf : Leaf
{
    public string CreatorId { get; }
    public string NamedId { get; }

    public DeferedLeafResolver(string creatorId, string namedId)
    {
        CreatorId = creatorId;
        NamedId = namedId;
    }

    public TLeaf Resolve() => RegistryResolver.Resolve<TLeaf>().Get(CreatorId, NamedId);
}