using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves;

public readonly struct Branch<TLeaf> : ILeafId, IEquatable<Branch<TLeaf>>
    where TLeaf : Leaf
{
    public string NamedId { get; }
    public string CreatorId { get; }
    internal string EffectiveId { get; }

    private readonly Lazy<TLeaf> _leaf;

    public Branch(TLeaf leaf)
    {
        NamedId = leaf.NamedId;
        CreatorId = leaf.CreatorId;
        EffectiveId = leaf.EffectiveId;
        _leaf = new(() => leaf);
    }

    public Branch(string creatorId, string namedId)
    {
        NamedId = namedId;
        CreatorId = creatorId;
        string effectiveId = EffectiveLeafId.CreateFromParts(creatorId, namedId);
        EffectiveId = effectiveId;
        _leaf = new(() => RegistryResolver.Resolve<TLeaf>().GetByEffectiveId(effectiveId));
    }

    public TLeaf Resolve() => _leaf.Value;

    public override int GetHashCode() => EffectiveId.GetHashCode();
    public override bool Equals(object? obj) => obj is Branch<TLeaf> other && Equals(other);

    public bool Equals(Branch<TLeaf> other) => EqualityComparer<string>.Default.Equals(EffectiveId, other.EffectiveId);

    public static bool operator ==(Branch<TLeaf> left, Branch<TLeaf> right) =>
        left.EffectiveId.Equals(right.EffectiveId);

    public static bool operator !=(Branch<TLeaf> left, Branch<TLeaf> right) =>
        !left.EffectiveId.Equals(right.EffectiveId);

    public static implicit operator Branch<TLeaf>(TLeaf leaf) => new(leaf);
}