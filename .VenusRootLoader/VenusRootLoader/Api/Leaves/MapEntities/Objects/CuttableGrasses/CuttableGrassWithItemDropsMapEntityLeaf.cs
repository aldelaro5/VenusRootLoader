using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithItemDropsMapEntityLeaf : CuttableGrassMapEntityLeaf
{
    internal CuttableGrassWithItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _itemsDroppedWhenCut = new(
            InternalVectorData,
            0,
            x => new(new(x?.Resolve().GameId ?? -1, x is null ? 1 : 0, 0)));
    }

    private readonly ListRefWrapper<Branch<ItemLeaf>?, Vector3> _itemsDroppedWhenCut;
    public IList<Branch<ItemLeaf>?> ItemsDroppedWhenCut => _itemsDroppedWhenCut;

    public Branch<FlagLeaf>? FlagSetToTrueWhenCutOrCollectingDroppedItem
    {
        get;
        set
        {
            InternalActivationFlagId = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, IList<Branch<ItemLeaf>?> itemsDroppedWhenCut)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(-1)]);
        foreach (Branch<ItemLeaf>? item in itemsDroppedWhenCut)
            ItemsDroppedWhenCut.Add(item);
    }

    internal override void InitializeFromExisting()
    {
        ILeavesRegistry<ItemLeaf> itemsRegistry = RegistryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();

        _itemsDroppedWhenCut.SynchronizeFromExistingData(
            InternalVectorData
                .Select(v => v.Value.x < 0
                    ? (Branch<ItemLeaf>?)null
                    : new Branch<ItemLeaf>(itemsRegistry.GetByGameId((int)v.Value.x)))
                .ToList());

        if (InternalActivationFlagId >= 0)
            FlagSetToTrueWhenCutOrCollectingDroppedItem = new(flagsRegistry.GetByGameId(InternalActivationFlagId));
    }
}