using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves;

public sealed record Branch<TLeaf> : ILeafResolver<TLeaf>
    where TLeaf : Leaf
{
    public string CreatorId => _resolver.CreatorId;
    public string NamedId => _resolver.NamedId;

    private readonly ILeafResolver<TLeaf> _resolver;

    public Branch(TLeaf leaf) => _resolver = new ImmediateLeafResolver<TLeaf>(leaf);
    public Branch(string creatorId, string namedId) => _resolver = new DeferedLeafResolver<TLeaf>(creatorId, namedId);

    public TLeaf Resolve() => _resolver.Resolve();

    public override int GetHashCode() => HashCode.Combine(CreatorId, NamedId);

    public bool Equals(Branch<TLeaf>? other)
    {
        if (other is null)
            return false;

        return EqualityComparer<string>.Default.Equals(CreatorId, other.CreatorId)
               && EqualityComparer<string>.Default.Equals(NamedId, other.NamedId);
    }

    public static implicit operator Branch<TLeaf>(TLeaf leaf) => new(leaf);
}