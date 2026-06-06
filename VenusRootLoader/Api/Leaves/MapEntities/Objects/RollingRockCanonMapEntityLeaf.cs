using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
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
            if (value?.MapEntity.Leaf.Map is { } map && map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShot),
                    $"The entity must be in the {map.NamedId} map");
            }

            if (value is { EffectiveValue: -1 })
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShot),
                    $"It is not possible to test for the {nameof(MapEntityLeaf)} with gameId 1 to be inactive because it is " +
                    $"internally equivalent of having a {nameof(RequiredMapEntityActivationForShot)} of null");
            }

            InternalData[3].Value = value?.EffectiveValue ?? -1;
            field = value;
        }
    }

    public Vector3 DestinationPosition
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public float MinimumYPositionBeforeRespawn
    {
        get => InternalVectorData[1].Value.x;
        set => InternalVectorData[1].Value.x = value;
    }

    public float? RockRadiusOverride
    {
        get => InternalVectorData[1].Value.y < 0.1 ? null : InternalVectorData[1].Value.y;
        set => InternalVectorData[1].Value.y = value is null or < 0.1f ? 0f : value.Value;
    }

    public float DelayFramesBeforeShot
    {
        get => InternalVectorData[1].Value.z;
        set => InternalVectorData[1].Value.z = value;
    }

    public Vector3 RollingRotationAngles
    {
        get => InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(0), new(1), new(-1)]);
        InternalVectorData.AddRange(
            [new(new(10f, 0f, 0f)), new(new(-10f, 0f, 0f)), new(new(10f, 0f, 0f)), new(new(0f, 0f, 5f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.RollingRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 4 - InternalData.Count));

        if (InternalData[3].Value != -1)
        {
            MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
            RequiredMapEntityActivationForShot = new()
            {
                MapEntity = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[3].Value)],
                IsActivationValueNegated = InternalData[3].Value < 0
            };
        }
    }
}