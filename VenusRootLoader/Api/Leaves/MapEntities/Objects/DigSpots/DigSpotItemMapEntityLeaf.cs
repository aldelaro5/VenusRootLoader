using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotItemMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<ItemLeaf> ItemHiddenInside
    {
        get;
        set
        {
            InternalData[2].Value = value.GameId;
            field = value;
        }
    }

    public bool IsHiddenItemAKeyItem
    {
        get => InternalData[1].Value == 1;
        set => InternalData[1].Value = value ? 1 : 0;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<ItemLeaf> itemHiddenInside,
        bool isHiddenItemAKeyItem)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(0), new(0)]);
        ItemHiddenInside = itemHiddenInside;
        IsHiddenItemAKeyItem = isHiddenItemAKeyItem;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ItemHiddenInside = new(itemsRegistry.LeavesByGameIds[InternalData[2].Value]);
    }
}