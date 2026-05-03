using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class MovableRockMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PushRock;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public float LaunchYVelocity
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new Vector3(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public float LaunchXZVelocityMultiplier
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new Vector3(InternalVectorData[0].x, InternalVectorData[0].y, value);
    }

    internal MovableRockMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, 0, 0]);
        InternalVectorData.AddRange([new(0f, 10f, 5f), new(0f, 0f, 0f)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PushRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(0, 4 - InternalData.Count));
        if (InternalVectorData.Count < 2)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 2 - InternalVectorData.Count));
    }
}