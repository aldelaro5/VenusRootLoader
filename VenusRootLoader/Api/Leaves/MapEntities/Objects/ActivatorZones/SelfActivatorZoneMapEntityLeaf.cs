using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.ActivatorZones.Enums;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.ActivatorZones;

public sealed class SelfActivatorZoneMapEntityLeaf : ActivatorZoneMapEntityLeaf
{
    internal SelfActivatorZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public SelfActivatorZoneMode ActivatorMode
    {
        get => (SelfActivatorZoneMode)InternalData[1].Value;
        set => InternalData[1].Value = (int)value;
    }

    public bool DestroysBeemerangWhileInsideAndDeactivated
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter,
        SelfActivatorZoneMode activatorMode)
    {
        base.InitializeFromNew(startingPosition, triggerBoxColliderSize, triggerBoxColliderCenter);
        InternalData.AddRange([new(-1), new(1), new(0)]);
        ActivatorMode = activatorMode;
    }
}