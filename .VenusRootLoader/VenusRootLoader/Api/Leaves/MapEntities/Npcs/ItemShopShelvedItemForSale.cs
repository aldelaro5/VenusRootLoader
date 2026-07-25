using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemShopShelvedItemForSale
{
    internal readonly Ref<int> RefItemGameId = new(0);
    internal readonly Ref<Vector3> RefPosition = new(Vector3.zero);

    public required Branch<ItemLeaf> Item
    {
        get;
        set
        {
            field = value;
            RefItemGameId.Value = value.GameId;
        }
    }

    public required Vector3 Position
    {
        get;
        set
        {
            field = value;
            RefPosition.Value = value;
        }
    }
}