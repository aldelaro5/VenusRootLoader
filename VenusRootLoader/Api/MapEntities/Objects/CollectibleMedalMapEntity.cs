using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class CollectibleMedalMapEntity : MapEntity
{
    internal CollectibleMedalMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Item;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Branch<MedalLeaf> Medal
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
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
        InternalData.AddRange([2, -1, 0]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.Add(-1);
        if (InternalData.Count < 3)
            InternalData.Add(0);

        ILeavesRegistry<MedalLeaf> medalsRegistry = registryResolver.Resolve<MedalLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        Medal = new(medalsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
        if (InternalData[1] > -1)
            EventToTriggerWhenCollected = new(eventsRegistry.LeavesByGameIds[InternalData[1]]);
        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}