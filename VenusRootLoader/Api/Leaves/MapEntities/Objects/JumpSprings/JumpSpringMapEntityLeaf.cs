using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.JumpSprings;

public abstract class JumpSpringMapEntityLeaf : MapEntityLeaf
{
    protected JumpSpringMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.JumpSpring;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public JumpSpringColor Color
    {
        get => InternalAnimIdOrItemId == (int)MainManager.AnimIDs.BounceShroom - 1
            ? JumpSpringColor.Green
            : JumpSpringColor.Red;
        set => InternalAnimIdOrItemId = value == JumpSpringColor.Green
            ? (int)MainManager.AnimIDs.BounceShroom - 1
            : (int)MainManager.AnimIDs.BounceShroom2 - 1;
    }

    internal override void InitializeFromNew()
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.5f, 1f, 1.5f);
        InternalBoxColCenter = new(0f, 0.4f, 0f);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BounceShroom - 1;
    }
}