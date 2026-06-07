namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public abstract class EventTriggerMapEntityLeaf : MapEntityLeaf
{
    protected EventTriggerMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;
}