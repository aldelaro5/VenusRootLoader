using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class FreezableWaterDropletMapEntityLeaf : ObjectMapEntityLeaf
{
    internal FreezableWaterDropletMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Dropplet;

    public int FramesDelayBeforeDropletRespawnAndIceCubeShatter
    {
        get => InternalData[0].Value;
        set => InternalData[0].Value = value;
    }

    public int? IceCubeMaxDistanceInUnitsFromOffsetBeforeShatter
    {
        get => InternalData[1].Value > 0 ? InternalData[1].Value : null;
        set => InternalData[1].Value = value ?? -1;
    }

    public int? IceCubeFramesWhileFrozenBeforeShatter
    {
        get => (InternalData[2].Value, InternalData[3].Value) switch
        {
            (1, > 0) => InternalData[3].Value,
            (0, > 0) => 0,
            (_, _) => null
        };
        set
        {
            if (value is not null)
            {
                if (value == 0)
                {
                    InternalData[2].Value = 0;
                    InternalData[3].Value = 1;
                }
                else
                {
                    InternalData[2].Value = 1;
                    InternalData[3].Value = value.Value;
                }
            }
            else
            {
                InternalData[2].Value = 1;
                InternalData[3].Value = 0;
            }
        }
    }

    public bool IceCubeCanOnlyMoveIn8CardinalDirections
    {
        get => InternalData[4].Value == 1;
        set => InternalData[4].Value = value ? 1 : 0;
    }

    public Vector3 IceCubeStartingPositionOffset
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public float IceCubeLaunchXZVelocity
    {
        get => InternalVectorData[1].Value.x;
        set => InternalVectorData[1].Value.x = value;
    }

    public float IceCubeLaunchYVelocity
    {
        get => InternalVectorData[1].Value.y;
        set => InternalVectorData[1].Value.y = value;
    }

    public float? WaterSplashSoundVolumeFraction
    {
        get => InternalVectorData[1].Value.z != 0f ? InternalVectorData[1].Value.z : null;
        set => InternalVectorData[1].Value.z = value ?? 0f;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(30), new(10), new(1), new(0), new(0)]);
        InternalVectorData.AddRange([new(Vector3.zero), new(new(5f, 10f, 0f))]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 2f, 2f);
        InternalBoxColCenter = new(0f, 1f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5 - InternalData.Count));
    }
}