namespace VenusRootLoader.Api.Leaves;

public readonly struct Branch<TLeaf> : ILeafIdentifier, IEquatable<Branch<TLeaf>>
    where TLeaf : Leaf, new()
{
    public int GameId => Leaf.GameId;
    public string NamedId => Leaf.NamedId;
    public string CreatorId => Leaf.CreatorId;

    internal TLeaf Leaf { get; }

    public Branch(TLeaf leaf) => Leaf = leaf;

    public override int GetHashCode() => Leaf.GetHashCode();
    public override bool Equals(object? obj) => Leaf.Equals(obj);
    public bool Equals(Branch<TLeaf> other) => EqualityComparer<TLeaf>.Default.Equals(Leaf, other.Leaf);
    public static bool operator ==(Branch<TLeaf> left, Branch<TLeaf> right) => left.Leaf.Equals(right.Leaf);
    public static bool operator !=(Branch<TLeaf> left, Branch<TLeaf> right) => !left.Leaf.Equals(right.Leaf);
}