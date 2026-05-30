using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class CuttableGrassMapEntityLeaf : MapEntityLeaf
{
    internal CuttableGrassMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Vector3 BoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 BoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    public int GrassSpriteId { get => InternalData[0]; set => InternalData[0] = value; }

    public Branch<CrystalBerryLeaf>? CrystalBerryDroppedWhenCut
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    public ReadOnlyCollection<Branch<ItemLeaf>?> ItemsDroppedWhenCut { get; private set; } =
        new List<Branch<ItemLeaf>?>().AsReadOnly();

    public int RegionalFlagId { get => InternalRegionalFlagId; set => InternalRegionalFlagId = value; }

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
        InternalData.AddRange([0, -1]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalData[1] >= 0)
            CrystalBerryDroppedWhenCut = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[1]]);

        List<Branch<ItemLeaf>?> itemsWhenCut = InternalVectorData
            .Select(v => v.x < 0
                ? (Branch<ItemLeaf>?)null
                : new Branch<ItemLeaf>(itemsRegistry.LeavesByGameIds[(int)v.x]))
            .ToList();
        ChangeItemsDroppedWhenCut(itemsWhenCut);

        if (InternalActivationFlagId >= 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }

    public void ChangeItemsDroppedWhenCut(List<Branch<ItemLeaf>?> items)
    {
        InternalVectorData.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            int x = items[i]?.GameId ?? -1;
            if (i < OriginalVectorData.Length)
                InternalVectorData.Add(new(x, OriginalVectorData[i].y, OriginalVectorData[i].z));
            else
                InternalVectorData.Add(new(x, 0f, 0f));
        }
        ItemsDroppedWhenCut = items.AsReadOnly();
    }
}