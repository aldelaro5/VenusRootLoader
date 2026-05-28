using UnityEngine;
using VenusRootLoader.Api.Common;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class JumpToPositionSpringMapEntity : MapEntity
{
    internal JumpToPositionSpringMapEntity(int gameId, string namedId, string creatorId)
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

    public int? InsideTransitionMapEntityIdToTriggerWhenUsingSpring
    {
        get => InternalData[2] < 0 ? null : InternalData[2];
        set => InternalData[2] = value ?? -1;
    }

    public Vector3 PositionToGoWhenUsingSpring
    {
        get => InternalVectorData[1];
        set => InternalVectorData[1] = value;
    }

    public float JumpHeightWhenUsingSpring
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float JumpDurationDivisorWhenUsingSpring
    {
        get => Mathf.Clamp(InternalVectorData[2].x, 1f, 99f);
        set => InternalVectorData[2] = new(
            Mathf.Clamp(value, 1f, 99f),
            InternalVectorData[0].y,
            InternalVectorData[0].z);
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([1, 0, -1]);
        InternalVectorData.AddRange([new(15f, 0f, 0f), Vector3.zero, Vector3.right]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.5f, 1f, 1.5f);
        InternalBoxColCenter = new(0f, 0.4f, 0f);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BounceShroom - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.Add(Vector3.right);
    }
}