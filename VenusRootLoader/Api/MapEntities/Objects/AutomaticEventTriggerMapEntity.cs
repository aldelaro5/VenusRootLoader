using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class AutomaticEventTriggerMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Branch<EventLeaf> EventToImmediatelyStart
    {
        get;
        set
        {
            InternalData[0] = value.GameId;
            field = value;
        }
    }

    internal AutomaticEventTriggerMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, 0, 1]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
        EventToImmediatelyStart = new(eventsRegistry.LeavesByGameIds[InternalData[0]]);
    }
}