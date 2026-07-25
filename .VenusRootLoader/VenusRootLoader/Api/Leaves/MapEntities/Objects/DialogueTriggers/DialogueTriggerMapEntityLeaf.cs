using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public abstract class DialogueTriggerMapEntityLeaf : ObjectMapEntityLeaf
{
    protected DialogueTriggerMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;
}