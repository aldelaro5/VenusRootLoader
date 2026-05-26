using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class CollectibleCrystalBerryMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Branch<CrystalBerryLeaf> CrystalBerry
    {
        get;
        set
        {
            InternalData[3] = value.GameId;
            field = value;
        }
    }

    public Branch<EventLeaf>? EventToTriggerWhenCollected
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    public bool IsCatchableByBeemerang
    {
        get => InternalData[2] == 0;
        set => InternalData[2] = value ? 0 : 1;
    }

    internal CollectibleCrystalBerryMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([3, -1, 0, 0]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        CrystalBerry = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[3]]);
        if (InternalData[1] > -1)
            EventToTriggerWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1]]);
    }
}