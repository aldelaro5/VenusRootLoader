using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyItemDrop
{
    internal readonly Ref<Vector3> Vector3Ref = new(new(0, -1, 0));

    public required Branch<ItemLeaf> Item
    {
        get;
        set
        {
            Vector3Ref.Value.x = value.GameId;
            field = value;
        }
    }

    public required Branch<FlagLeaf>? RequiredFlag
    {
        get;
        set
        {
            Vector3Ref.Value.y = value?.GameId ?? -1;
            field = value;
        }
    }
}