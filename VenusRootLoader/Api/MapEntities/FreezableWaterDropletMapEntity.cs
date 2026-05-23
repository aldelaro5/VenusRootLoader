using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class FreezableWaterDropletMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Dropplet;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int FramesDelayBeforeDropletRespawnAndIceCubeShatter
    {
        get => InternalData[0];
        set => InternalData[0] = value;
    }

    public int? IceCubeMaxDistanceInUnitsFromOffsetBeforeShatter
    {
        get => InternalData[1] > 0 ? InternalData[1] : null;
        set => InternalData[1] = value ?? -1;
    }

    public int? IceCubeFramesWhileFrozenBeforeShatter
    {
        get => (InternalData[2], InternalData[3]) switch
        {
            (1, > 0) => InternalData[3],
            (0, > 0) => 0,
            (_, _) => null
        };
        set
        {
            if (value is not null)
            {
                if (value == 0)
                {
                    InternalData[2] = 0;
                    InternalData[3] = 1;
                }
                else
                {
                    InternalData[2] = 1;
                    InternalData[3] = value.Value;
                }
            }
            else
            {
                InternalData[2] = 1;
                InternalData[3] = 0;
            }
        }
    }

    public bool IceCubeCanOnlyMoveIn8CardinalDirections
    {
        get => InternalData[4] == 1;
        set => InternalData[4] = value ? 1 : 0;
    }

    public Vector3 IceCubeStartingPositionOffset { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }

    public float IceCubeLaunchXZVelocity
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public float IceCubeLaunchYVelocity
    {
        get => InternalVectorData[1].y;
        set => InternalVectorData[1] = new(InternalVectorData[1].x, value, InternalVectorData[1].z);
    }

    public float? WaterSplashSoundVolumeFraction
    {
        get => InternalVectorData[1].z != 0f ? InternalVectorData[1].z : null;
        set => InternalVectorData[1] = new(InternalVectorData[1].x, InternalVectorData[1].y, value ?? 0f);
    }

    internal FreezableWaterDropletMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([30, 10, 1, 0, 0]);
        InternalVectorData.AddRange([Vector3.zero, new(5f, 10f, 0f)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 2f, 2f);
        InternalBoxColCenter = new(0f, 1f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(0, 5 - InternalData.Count));
    }
}