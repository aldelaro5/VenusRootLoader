using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class ItemShopShelvedItemForSale
{
    public required Branch<ItemLeaf> Item { get; set; }
    public required Vector3 Position { get; set; }
}