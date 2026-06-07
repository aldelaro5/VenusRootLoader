using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class WindBeamZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal WindBeamZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.WindPusher;

    public Branch<MapEntityLeaf>? RequiredMapEntityActivation
    {
        get;
        set
        {
            if (value?.Leaf.Map is { } map && map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivation),
                    $"The entity is not in the {map.NamedId} map which is required");
            }

            InternalData[0].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 EndPosition { get => InternalVectorData[0].Value; set => InternalVectorData[0].Value = value; }

    public float WindPushForceUnitsPerSecond
    {
        get => InternalVectorData[1].Value.x;
        set => InternalVectorData[1].Value.x = value;
    }

    public float ColliderWidth
    {
        get => InternalVectorData[1].Value.y;
        set => InternalVectorData[1].Value.y = value;
    }

    public float ColliderHeight
    {
        get => InternalVectorData[1].Value.z;
        set => InternalVectorData[1].Value.z = value;
    }

    public float? WindParticlesStartLifetimeOverride
    {
        get => InternalVectorData[2].Value.x < 0.1f ? null : InternalVectorData[2].Value.x;
        set => InternalVectorData[2].Value.x = value is null or < 0.1f ? 0f : value.Value;
    }

    public float? WindParticlesStartSpeedOverride
    {
        get => InternalVectorData[2].Value.y < 0.1f ? null : InternalVectorData[2].Value.y;
        set => InternalVectorData[2].Value.y = value is null or < 0.1f ? 0f : value.Value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.Add(new(-1));
        InternalVectorData.AddRange([new(new(1f, 0f, 0f)), new(new(0.1f, 3f, 3f)), new(new(0f, 0f, 0f))]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalVectorData.Count));
        }

        if (InternalData[0].Value != -1)
            RequiredMapEntityActivation = Map.Leaf.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[0].Value)];
    }
}