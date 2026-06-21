using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: check for wooden switch support
public sealed class IceRadiusSwitchMapEntityLeaf : ObjectMapEntityLeaf
{
    internal IceRadiusSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.StencilSwitch;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public Branch<MapEntityLeaf>? ParentMapEntity
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public bool IsActivatedOnMapLoad
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    public bool AllCollidersAreDisabled
    {
        get => InternalData[3].Value == 1;
        set => InternalData[3].Value = value ? 1 : 0;
    }

    public float RadiusRangeChangeRateWhenToggled
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float RadiusRange
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public Vector3 LocalPositionFromMapEntityParent
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? FlagSwitchActivation
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        float radiusRange,
        float radiusRangeChangeRateWhenToggled)
    {
        InternalData.AddRange([new(0), new(-1), new(0), new(0)]);
        InternalVectorData.AddRange([new(new(0.1f, 5f, 0f)), new(new(0f, 0f, 0f))]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        RadiusRange = radiusRange;
        RadiusRangeChangeRateWhenToggled = radiusRangeChangeRateWhenToggled;
        EntityStartingPosition = startingPosition;
        AnimId = animId;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalActivationFlagId > 0)
            FlagSwitchActivation = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);

        if (InternalData[1].Value != -1)
            ParentMapEntity = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[1].Value];
    }
}