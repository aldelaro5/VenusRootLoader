using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithItemDropsMapEntityLeaf : MapEntityLeaf
{
    internal CuttableGrassWithItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _itemsDroppedWhenCut = new(InternalVectorData, 0, x => new(new(x?.GameId ?? -1, x is null ? 1 : 0, 0)));
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Vector3 BoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 BoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    public int GrassSpriteId { get => InternalData[0].Value; set => InternalData[0].Value = value; }

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
        InternalData.AddRange([new(0), new(-1)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
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