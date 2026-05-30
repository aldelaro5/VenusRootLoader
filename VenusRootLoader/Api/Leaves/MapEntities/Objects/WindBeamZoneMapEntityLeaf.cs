using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class WindBeamZoneMapEntityLeaf : MapEntityLeaf
{
    internal WindBeamZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.WindPusher;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

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

            InternalData[0] = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 EndPosition { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }

    public float WindPushForceUnitsPerSecond
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public float ColliderWidth
    {
        get => InternalVectorData[1].y;
        set => InternalVectorData[1] = new(InternalVectorData[1].x, value, InternalVectorData[1].z);
    }

    public float ColliderHeight
    {
        get => InternalVectorData[1].z;
        set => InternalVectorData[1] = new(InternalVectorData[1].x, InternalVectorData[1].y, value);
    }

    public float? WindParticlesStartLifetimeOverride
    {
        get => InternalVectorData[2].x < 0.1f ? null : InternalVectorData[2].x;
        set => InternalVectorData[2] = new(
            value is null or < 0.1f ? 0f : value.Value,
            InternalVectorData[2].y,
            InternalVectorData[2].z);
    }

    public float? WindParticlesStartSpeedOverride
    {
        get => InternalVectorData[2].y < 0.1f ? null : InternalVectorData[2].y;
        set => InternalVectorData[2] = new(
            InternalVectorData[2].x,
            value is null or < 0.1f ? 0f : value.Value,
            InternalVectorData[2].z);
    }

    internal override void InitializeFromNew()
    {
        InternalData.Add(-1);
        InternalVectorData.AddRange([new(1f, 0f, 0f), new(0.1f, 3f, 3f), new(0f, 0f, 0f)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalVectorData.Count));

        if (InternalData[0] != -1)
            RequiredMapEntityActivation = Map.Leaf.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[0])];
    }
}