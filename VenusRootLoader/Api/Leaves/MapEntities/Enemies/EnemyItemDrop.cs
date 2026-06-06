using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyItemDrop : IHasUnderluingValue<Vector3>
{
    private readonly Vector3 _ref = Vector3.zero;

    Vector3 IHasUnderluingValue<Vector3>.UnderlyingRef => _ref;

    public required Branch<ItemLeaf> Item
    {
        get;
        init
        {
            _ref.x = value.GameId;
            field = value;
        }
    }

    public required Branch<FlagLeaf>? RequiredFlag
    {
        get;
        init
        {
            _ref.y = value?.GameId ?? -1;
            field = value;
        }
    }
}