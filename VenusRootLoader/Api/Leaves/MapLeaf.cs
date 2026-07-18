using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Figure out the MapControl config and Unity prefab tooling
[ExposeFromVenus(withRegisterMethod: false)]
public sealed class MapLeaf : Leaf
{
    internal MapLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Func<MapLeaf, GameObject>? PrefabInstantiator { get; set; }

    internal ILeavesRegistry<MapEntityLeaf> EntitiesRegistry { get; set; } = null!;
    internal ILeavesRegistry<MapDialogueLeaf> DialoguesRegistry { get; set; } = null!;
}