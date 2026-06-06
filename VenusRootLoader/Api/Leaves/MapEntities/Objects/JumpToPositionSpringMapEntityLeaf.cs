using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class JumpToPositionSpringMapEntityLeaf : MapEntityLeaf
{
    internal JumpToPositionSpringMapEntityLeaf(int gameId, string namedId, string creatorId)
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
        get => InternalData[2].Value < 0 ? null : InternalData[2].Value;
        set => InternalData[2].Value = value ?? -1;
    }

    public Vector3 PositionToGoWhenUsingSpring
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public float JumpHeightWhenUsingSpring
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float JumpDurationDivisorWhenUsingSpring
    {
        get => Mathf.Clamp(InternalVectorData[2].Value.x, 1f, 99f);
        set => InternalVectorData[2].Value.x = Mathf.Clamp(value, 1f, 99f);
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(1), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(15f, 0f, 0f)), new(Vector3.zero), new(Vector3.right)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.5f, 1f, 1.5f);
        InternalBoxColCenter = new(0f, 0.4f, 0f);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BounceShroom - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.Add(new(Vector3.right));
    }
}