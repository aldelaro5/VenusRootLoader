using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class EventTriggerAutomaticMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;

    public Branch<EventLeaf> EventToImmediatelyStart
    {
        get;
        set
        {
            InternalData[0] = value.GameId;
            field = value;
        }
    }

    internal EventTriggerAutomaticMapEntity() { }

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