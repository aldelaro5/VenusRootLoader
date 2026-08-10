using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.LeavesInternals;

/// <summary>
/// A scheme to resolve a <see cref="Leaf"/> from a <see cref="Branch{TLeaf}"/>.
/// </summary>
/// <typeparam name="TLeaf">The type of leaf this resolver resolves.</typeparam>
internal interface ILeafResolver<TLeaf> : ILeafId
    where TLeaf : Leaf
{
    /// <summary>
    /// Obtains the leaf referenced by this resolver.
    /// </summary>
    /// <returns>The leaf if it exists.</returns>
    /// <exception cref="System.ArgumentException">Thrown if the leaf does not exists.</exception>
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