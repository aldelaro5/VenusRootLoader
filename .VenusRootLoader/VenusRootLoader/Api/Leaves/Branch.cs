using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves;

/// <summary>
/// An immutable reference to a <see cref="Leaf"/> which is either referencing an existing leaf that can be resolved at
/// any time or is resolved at a later time using a <see cref="Leaf.CreatorId"/> and <see cref="Leaf.NamedId"/>.
/// </summary>
/// <typeparam name="TLeaf">The type of leaf this branch references.</typeparam>
/// <remarks>
/// This record is implicitly convertible from a <see cref="Leaf"/> which yields a Branch of that leaf. The leaf will
/// be immediately resolvable, and it is the equivalent of using the constrcutor that accepts a <typeparamref name="TLeaf"/>
/// </remarks>
public sealed record Branch<TLeaf> : ILeafResolver<TLeaf>
    where TLeaf : Leaf
{
    public string CreatorId => _resolver.CreatorId;
    public string NamedId => _resolver.NamedId;

    private readonly ILeafResolver<TLeaf> _resolver;

    /// <summary>
    /// Create a branch that will reference an existing leaf. This is implicitly invoked when an implicit conversion from
    /// <typeparamref name="TLeaf"/> to <see cref="Branch{TLeaf}"/> happens.
    /// </summary>
    /// <param name="leaf">The leaf to reference in the new branch.</param>
    public Branch(TLeaf leaf) => _resolver = new ImmediateLeafResolver<TLeaf>(leaf);

    /// <summary>
    /// Creates a branch that will reference a leaf that may or may not exist and will only be resolved when <see cref="Resolve"/> is called.
    /// </summary>
    /// <param name="creatorId">The CreatorId of the leaf to reference.</param>
    /// <param name="namedId">The CreatorId of the leaf to reference.</param>
    /// <remarks>
    /// While the leaf may not exist the moment this constructor is called, the leaf must exist by the time <see cref="Resolve"/>
    /// is called. If it still doesn't exist, an exception will be thrown by <see cref="Resolve"/>. Use this constructor
    /// if you want to reference a leaf that can't otherwise be created the moment you need to reference it. It is aimed
    /// to address potential "chicken and egg" problems.
    /// </remarks>
    public Branch(string creatorId, string namedId) => _resolver = new DeferredLeafResolver<TLeaf>(creatorId, namedId);

    /// <summary>
    /// Obtains the leaf referenced by this branch. If the constructor taking a leaf was used to create this branch, this
    /// always returns the leaf that was passed to the constructor. If the constructor taking a CreatorId and NamedId was used
    /// to create this branch, It will be loaded from its respective registry.
    /// </summary>
    /// <returns>The leaf referenced by this branch.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the constructor taking a CreatorId and NamedId was used
    /// and the leaf referenced by this branch does not exist.</exception>
    public TLeaf Resolve() => _resolver.Resolve();

    public override int GetHashCode() => HashCode.Combine(CreatorId, NamedId);

    public bool Equals(Branch<TLeaf>? other)
    {
        if (other is null)
            return false;

        return EqualityComparer<string>.Default.Equals(CreatorId, other.CreatorId)
               && EqualityComparer<string>.Default.Equals(NamedId, other.NamedId);
    }

    /// <summary>
    /// Allows implicit conversion from a leaf to a branch referencing it.
    /// </summary>
    /// <param name="leaf">The leaf the new branch will reference.</param>
    /// <returns>A branch referencing <paramref name="leaf"/></returns>
    public static implicit operator Branch<TLeaf>(TLeaf leaf) => new(leaf);
}