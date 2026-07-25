namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public abstract class EventTriggerMapEntityLeaf : ObjectMapEntityLeaf
{
    protected EventTriggerMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;
}