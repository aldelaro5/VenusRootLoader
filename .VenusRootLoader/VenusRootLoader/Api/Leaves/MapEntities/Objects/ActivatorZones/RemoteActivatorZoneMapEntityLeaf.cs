using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.ActivatorZones.Enums;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.ActivatorZones;

public sealed class RemoteActivatorZoneMapEntityLeaf : ActivatorZoneMapEntityLeaf
{
    internal RemoteActivatorZoneMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<ObjectMapEntityLeaf> MapEntityWhoseActivationIsControlledByThis
    {
        get;
        set
        {
            if (value.Resolve().Map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityWhoseActivationIsControlledByThis),
                    $"The entity is not in the {Map.NamedId} map which is required");
            }

            if (value.Resolve().GameId == GameId)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityWhoseActivationIsControlledByThis),
                    "The entity controlled cannot be the entity itself");
            }

            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    public RemoteActivatorZoneMode ActivatorMode
    {
        get => (RemoteActivatorZoneMode)InternalData[1].Value;
        set => InternalData[1].Value = (int)value;
    }

    public bool DestroysBeemerangWhileInside
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter,
        Branch<ObjectMapEntityLeaf> mapEntityWhoseActivationIsControlledByThis,
        RemoteActivatorZoneMode activatorMode)
    {
        base.InitializeFromNew(startingPosition, triggerBoxColliderSize, triggerBoxColliderCenter);
        InternalData.AddRange([new(0), new(1), new(0)]);
        MapEntityWhoseActivationIsControlledByThis = mapEntityWhoseActivationIsControlledByThis;
        ActivatorMode = activatorMode;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        MapEntityWhoseActivationIsControlledByThis =
            (Branch<ObjectMapEntityLeaf>)Map.Resolve().EntitiesRegistry
                .GetByGameId(Math.Abs(InternalData[0].Value))!;
    }
}