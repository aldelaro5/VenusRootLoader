using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public sealed class RollingRockCanonMapEntityLeaf : RollingRockMapEntityLeaf
{
    internal RollingRockCanonMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public NegatableMapEntityActivation? RequiredMapEntityActivationForShooting
    {
        get;
        set
        {
            if (value?.MapEntity.Resolve().Map is { } map && map != Map)
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
        set
        {
            Guard.IsGreaterThan(value, 0f);
            InternalVectorData[1].Value.z = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 velocityWhenRolling)
    {
        base.InitializeFromNew(startingPosition, velocityWhenRolling);
        InternalData.AddRange([new(0), new(0), new(1), new(-1)]);
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        if (InternalData[3].Value != -1)
        {
            MapLeaf map = RegistryResolver.Resolve<MapLeaf>().GetByGameId(Map.Resolve().GameId);
            RequiredMapEntityActivationForShooting = new()
            {
                MapEntity = new(
                    (ObjectMapEntityLeaf)map.EntitiesRegistry.GetByGameId(Math.Abs(InternalData[3].Value))),
                IsActivationValueNegated = InternalData[3].Value < 0
            };
        }
    }
}