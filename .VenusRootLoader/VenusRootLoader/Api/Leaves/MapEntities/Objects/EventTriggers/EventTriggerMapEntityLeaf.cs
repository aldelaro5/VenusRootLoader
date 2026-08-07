namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public abstract class EventTriggerMapEntityLeaf : ObjectMapEntityLeaf
{
    protected EventTriggerMapEntityLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;
}