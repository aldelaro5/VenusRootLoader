using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

// TODO: Look into a way to fix the length 0 problem resulting in Vector3.zero which is a Crunchy Leaf drop
public sealed class EnemyItemDrop
{
    internal readonly Ref<Vector3> Vector3Ref = new(new(0, -1, 0));

    public required Branch<ItemLeaf>? Item
    {
        get;
        set
        {
            Vector3Ref.Value.x = value?.GameId ?? -1;
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