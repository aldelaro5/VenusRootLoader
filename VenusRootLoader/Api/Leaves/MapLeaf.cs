using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves;

// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
    internal MapLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal ILeavesRegistry<MapEntityLeaf> EntitiesRegistry { get; set; } = null!;
    internal ILeavesRegistry<MapDialogueLeaf> DialoguesRegistry { get; set; } = null!;
}