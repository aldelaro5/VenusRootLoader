using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: it's possible this works with other animId, recheck later
public sealed class IceRadiusSwitchMapEntityLeaf : ObjectMapEntityLeaf
{
    internal IceRadiusSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.StencilSwitch;

    public IceRadiusSwitchKind SwitchKind
    {
        get => (IceRadiusSwitchKind)InternalAnimIdOrItemId;
        set => InternalAnimIdOrItemId = (int)value;
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

    public bool IsActuatedOnMapLoad { get => InternalData[2].Value == 1; set => InternalData[2].Value = value ? 1 : 0; }

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

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(-1), new(0), new(0)]);
        InternalVectorData.AddRange([new(new(0.1f, 5f, 0f)), new(new(0f, 0f, 0f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BigCrystalSwitch - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);

        if (InternalData[1].Value != -1)
            ParentMapEntity = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[1].Value];
    }
}