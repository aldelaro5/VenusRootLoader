using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.LeavesInternals;

/// <summary>
/// A scheme to resolve a <see cref="Leaf"/> from a <see cref="Branch{TLeaf}"/>.
/// </summary>
/// <typeparam name="TLeaf">The type of leaf this resolver resolves.</typeparam>
internal interface ILeafResolver<out TLeaf> : ILeafId
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

internal sealed class DeferredLeafResolver<TLeaf> : ILeafResolver<TLeaf>
    where TLeaf : Leaf
{
    public string CreatorId { get; }
    public string NamedId { get; }

    public DeferredLeafResolver(string creatorId, string namedId)
    {
        CreatorId = creatorId;
        NamedId = namedId;
    }

    public TLeaf Resolve() => RegistryResolver.Resolve<TLeaf>().Get(CreatorId, NamedId);
}

internal sealed class DeferredMapDialogueLeafResolver : ILeafResolver<MapDialogueLeaf>
{
    public string CreatorId { get; }
    public string NamedId { get; }
    private readonly MapLeaf _mapLeaf;

    public DeferredMapDialogueLeafResolver(MapLeaf mapLeaf, string creatorId, string namedId)
    {
        CreatorId = creatorId;
        NamedId = namedId;
        _mapLeaf = mapLeaf;
    }

    public MapDialogueLeaf Resolve() => _mapLeaf.DialoguesRegistry.Get(CreatorId, NamedId);
}