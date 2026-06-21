using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public sealed class RollingRockCanonMapEntityLeaf : RollingRockMapEntityLeaf
{
    internal RollingRockCanonMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public NegatableMapEntityActivation? RequiredMapEntityActivationForShooting
    {
        get;
        set
        {
            if (value?.MapEntity.Leaf.Map is { } map && map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShooting),
                    $"The entity must be in the {map.NamedId} map");
            }

            if (value is { EffectiveValue: -1 })
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(RequiredMapEntityActivationForShooting),
                    $"It is not possible to test for the {nameof(MapEntityLeaf)} with gameId 1 to be inactive because it is " +
                    $"internally equivalent of having a {nameof(RequiredMapEntityActivationForShooting)} of null");
            }

            InternalData[3].Value = value?.EffectiveValue ?? -1;
            field = value;
        }
    }

    public float DelayFramesBeforeShooting
    {
        get => InternalVectorData[1].Value.z;
        set => InternalVectorData[1].Value.z = value;
    }

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 velocityWhenRolling)
    {
        base.InitializeFromNew(startingPosition, velocityWhenRolling);
        InternalData.AddRange([new(0), new(0), new(1), new(-1)]);
        InternalVectorData.AddRange(
            [new(new(10f, 0f, 0f)), new(new(-10f, 0f, 0f)), new(new(10f, 0f, 0f)), new(new(0f, 0f, 5f))]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        if (InternalData[3].Value != -1)
        {
            MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
            RequiredMapEntityActivationForShooting = new()
            {
                MapEntity =
                    (Branch<ObjectMapEntityLeaf>)map.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[3].Value)]!,
                IsActivationValueNegated = InternalData[3].Value < 0
            };
        }
    }
}