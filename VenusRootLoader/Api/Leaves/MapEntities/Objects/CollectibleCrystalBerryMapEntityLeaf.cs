using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class CollectibleCrystalBerryMapEntityLeaf : MapEntityLeaf
{
    internal CollectibleCrystalBerryMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Branch<CrystalBerryLeaf> CrystalBerry
    {
        get;
        set
        {
            InternalData[3].Value = value.GameId;
            field = value;
        }
    }

    public Branch<EventLeaf>? EventToTriggerWhenCollected
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public bool IsCatchableByBeemerang
    {
        get => InternalData[2].Value == 0;
        set => InternalData[2].Value = value ? 0 : 1;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(3), new(-1), new(0), new(0)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        CrystalBerry = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[3].Value]);
        if (InternalData[1].Value > -1)
            EventToTriggerWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}