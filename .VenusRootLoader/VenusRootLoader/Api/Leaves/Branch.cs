using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves;

public readonly record struct Branch<TLeaf> : ILeafId
    where TLeaf : Leaf
{
    public string NamedId { get; }
    public string CreatorId { get; }

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

    public bool Equals(Branch<TLeaf> other) =>
        EqualityComparer<string>.Default.Equals(CreatorId, other.CreatorId)
        && EqualityComparer<string>.Default.Equals(NamedId, other.NamedId);

    public static implicit operator Branch<TLeaf>(TLeaf leaf) => new(leaf);
}