using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class CollectibleItemMapEntityLeaf : MapEntityLeaf
{
    internal CollectibleItemMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Branch<ItemLeaf> Item
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public bool IsKeyItem
    {
        get => InternalData[0] == 1;
        set => InternalData[0] = value ? 1 : 0;
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
        InternalData.AddRange([0, -1, 0]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.Add(-1);
        if (InternalData.Count < 3)
            InternalData.Add(0);

        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        Item = new(itemsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalData[1] > -1)
            EventToTriggerWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1]]);
        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}