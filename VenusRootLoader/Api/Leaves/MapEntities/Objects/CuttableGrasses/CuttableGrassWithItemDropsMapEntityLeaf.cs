using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithItemDropsMapEntityLeaf : CuttableGrassMapEntityLeaf
{
    internal CuttableGrassWithItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _itemsDroppedWhenCut = new(InternalVectorData, 0, x => new(new(x?.GameId ?? -1, x is null ? 1 : 0, 0)));
    }

    private readonly ListRefWrapper<Branch<ItemLeaf>?, Vector3> _itemsDroppedWhenCut;
    public IList<Branch<ItemLeaf>?> ItemsDroppedWhenCut => _itemsDroppedWhenCut;

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(-1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        _itemsDroppedWhenCut.SynchronizeFromExistingData(
            InternalVectorData
                .Select(v => v.Value.x < 0
                ? (Branch<ItemLeaf>?)null
                : new Branch<ItemLeaf>(itemsRegistry.LeavesByGameIds[(int)v.Value.x]))
                .ToList());

        if (InternalActivationFlagId >= 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}