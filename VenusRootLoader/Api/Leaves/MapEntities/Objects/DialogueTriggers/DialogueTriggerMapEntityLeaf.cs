namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public abstract class DialogueTriggerMapEntityLeaf : MapEntityLeaf
{
    protected DialogueTriggerMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;
}