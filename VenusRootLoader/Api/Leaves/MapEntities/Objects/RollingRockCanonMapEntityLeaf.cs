using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class RollingRockCanonMapEntityLeaf : MapEntityLeaf
{
    internal RollingRockCanonMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.RollingRock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public NegatableMapEntityActivation? RequiredMapEntityActivationForShot
    {
        get;
        set
        {
            if (value?.MapEntityLeaf.Map is { } map && map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShot),
                    $"The entity is not in the {map.NamedId} map which is required");
            }

            if (value is { EffectiveValue: -1 })
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShot),
                    $"It is not possible to test for the {nameof(MapEntityLeaf)} with id 1 to be inactive because it is " +
                    $"internally equivalent of having a {nameof(RequiredMapEntityActivationForShot)} of null");
            }

            InternalData[3] = value?.EffectiveValue ?? -1;
            field = value;
        }
    }

    public Vector3 DestinationPosition { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }

    public float MinimumYPositionBeforeRespawn
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public float? RockRadiusOverride
    {
        get => InternalVectorData[1].y < 0.1 ? null : InternalVectorData[1].y;
        set => InternalVectorData[1] = new(
            InternalVectorData[1].x,
            value is null or < 0.1f ? 0f : value.Value,
            InternalVectorData[1].z);
    }

    public float DelayFramesBeforeShot
    {
        get => InternalVectorData[1].z;
        set => InternalVectorData[1] = new(InternalVectorData[1].x, InternalVectorData[1].y, value);
    }

    public Vector3 RollingRotationAngles { get => InternalVectorData[2]; set => InternalVectorData[2] = value; }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, 1, -1]);
        InternalVectorData.AddRange([new(10f, 0f, 0f), new(-10f, 0f, 0f), new(10f, 0f, 0f), new(0f, 0f, 5f)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.RollingRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(-1, 4 - InternalData.Count));

        if (InternalData[3] != -1)
        {
            MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
            RequiredMapEntityActivationForShot = new()
            {
                MapEntityLeaf = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[3])],
                IsActivationValueNegated = InternalData[3] < 0
            };
        }
    }
}