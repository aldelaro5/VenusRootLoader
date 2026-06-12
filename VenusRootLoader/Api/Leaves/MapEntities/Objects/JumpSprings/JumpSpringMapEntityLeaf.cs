using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.JumpSprings;

public abstract class JumpSpringMapEntityLeaf : ObjectMapEntityLeaf
{
    protected JumpSpringMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.JumpSpring;

    public JumpSpringColor Color
    {
        get => InternalAnimIdOrItemId == (int)MainManager.AnimIDs.BounceShroom - 1
            ? JumpSpringColor.Green
            : JumpSpringColor.Red;
        set => InternalAnimIdOrItemId = value == JumpSpringColor.Green
            ? (int)MainManager.AnimIDs.BounceShroom - 1
            : (int)MainManager.AnimIDs.BounceShroom2 - 1;
    }

    protected void InitializeFromNew(Vector3 startingPosition)
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.5f, 1f, 1.5f);
        InternalBoxColCenter = new(0f, 0.4f, 0f);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BounceShroom - 1;
        EntityStartingPosition = startingPosition;
    }
}