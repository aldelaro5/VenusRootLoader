using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class SlidingIcePillarMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PushRock;

    public float SlidingXZVelocityMultiplier
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new Vector3(InternalVectorData[0].x, InternalVectorData[0].y, value);
    }

    public Vector3? IcePillarScaleOverride
    {
        get => InternalBoxColSize.magnitude <= 0.1f ? null : InternalBoxColSize;
        set => InternalBoxColSize = value is null || value.Value.magnitude <= 0.1f ? Vector3.zero : value.Value;
    }

    internal SlidingIcePillarMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, 3, 0]);
        InternalVectorData.AddRange([new(0f, 0f, 0.1f), new(0f, 0f, 0f)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColSize = Vector3.zero;
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.IcePillarObj - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(0, 4 - InternalData.Count));
    }
}