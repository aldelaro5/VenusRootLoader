using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class JumpUpSpringMapEntityLeaf : MapEntityLeaf
{
    internal JumpUpSpringMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.JumpSpring;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

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

    public float? JumpHeightOverrideWhenUsingSpring
    {
        get => InternalVectorData[0].x <= 1.0f ? null : InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value ?? 0f, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, -1]);
        InternalVectorData.AddRange([Vector3.right * 25f, Vector3.zero, Vector3.zero]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.5f, 1f, 1.5f);
        InternalBoxColCenter = new(0f, 0.4f, 0f);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BounceShroom - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}