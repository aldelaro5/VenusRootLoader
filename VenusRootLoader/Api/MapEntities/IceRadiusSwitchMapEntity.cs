using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

// TODO: it's possible this works with other animid, recheck later
public enum IceRadiusSwitchKind
{
    BigCrystalSwitch = 54,
    SwitchCrystal = 36
}

public sealed class IceRadiusSwitchMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.StencilSwitch;

    public IceRadiusSwitchKind SwitchKind
    {
        get => (IceRadiusSwitchKind)InternalAnimIdOrItemId;
        set => InternalAnimIdOrItemId = (int)value;
    }

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int? ParentMapEntityId
    {
        get => InternalData[1] == -1 ? null : InternalData[1];
        set => InternalData[1] = value ?? -1;
    }

    public bool IsActuatedOnMapLoad { get => InternalData[2] == 1; set => InternalData[2] = value ? 1 : 0; }
    public bool AllCollidersAreDisabled { get => InternalData[3] == 1; set => InternalData[3] = value ? 1 : 0; }

    public float RadiusChangeRateWhenToggled
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float RadiusRange
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public Vector3 LocalPositionFromMapEntityParent
    {
        get => InternalVectorData[1];
        set => InternalVectorData[1] = value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public int? RegionalFlagId
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal IceRadiusSwitchMapEntity()
    {
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, -1, 0, 0]);
        InternalVectorData.AddRange([new(0.1f, 5f, 0f), new(0f, 0f, 0f)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.BigCrystalSwitch - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}