using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

/// <summary>
/// A registry whose identification is based on another registry instead of having its own. The <see cref="Leaf.NamedId"/>
/// and <see cref="Leaf.GameId"/> will be derived from the other registry, but the <see cref="Leaf.CreatorId"/> will be
/// kept so buds can still filter leaves by creator id.
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type this registry manages.</typeparam>
/// <typeparam name="TKeyLeaf">The <see cref="Leaf"/> that represents the unique identification key for this registry.</typeparam>
internal sealed class BranchBasedRegistry<TLeaf, TKeyLeaf> : BaseRegistry<TLeaf>
    where TLeaf : Leaf
    where TKeyLeaf : Leaf
{
    private readonly ILeavesRegistry<TKeyLeaf> _keyLeavesRegistry;

    public BranchBasedRegistry(ILogger logger, ILeavesRegistry<TKeyLeaf> keyLeavesRegistry)
        : base(logger, keyLeavesRegistry.EffectiveIdComparer)
    {
        _keyLeavesRegistry = keyLeavesRegistry;
    }

    // We never create new game ids, we only pick existing ones based on the effective id of the key.
    protected override int CreateNewGameId(string effectiveId) =>
        _keyLeavesRegistry.GetByEffectiveId(effectiveId).GameId;
}