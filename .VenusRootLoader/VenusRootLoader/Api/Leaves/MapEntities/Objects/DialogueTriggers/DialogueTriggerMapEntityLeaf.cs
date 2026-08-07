namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public abstract class DialogueTriggerMapEntityLeaf : ObjectMapEntityLeaf
{
    protected DialogueTriggerMapEntityLeaf(int gameId, string creatorId, string namedId) : base(
        gameId,
        creatorId,
        namedId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;
}