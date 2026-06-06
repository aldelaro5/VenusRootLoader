using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class MovableRockMapEntityLeaf : MapEntityLeaf
{
    internal MovableRockMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PushRock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public float LaunchYVelocity
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float LaunchXZVelocityMultiplier
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 4));
        InternalVectorData.AddRange([new(new(0f, 10f, 5f)), new(new(0f, 0f, 0f))]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PushRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 4 - InternalData.Count));
        if (InternalVectorData.Count < 2)
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 2 - InternalVectorData.Count));
    }
}