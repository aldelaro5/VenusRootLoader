using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves;

public sealed record Branch<TLeaf> : ILeafId
    where TLeaf : Leaf
{
    public string CreatorId { get; }
    public string NamedId { get; }

    private readonly Lazy<TLeaf> _leaf;

    public Branch(TLeaf leaf)
    {
        NamedId = leaf.NamedId;
        CreatorId = leaf.CreatorId;
        _leaf = new(() => leaf);
    }

    public Branch(string creatorId, string namedId)
    {
        NamedId = namedId;
        CreatorId = creatorId;
        _leaf = new(() => RegistryResolver.Resolve<TLeaf>().Get(creatorId, namedId));
    }

    public TLeaf Resolve() => _leaf.Value;

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