using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public enum SwitchTriggerZoneMode
{
    ActivateOnLeave = 0,
    ActivateOnEnterDeactivateOnLeave = 1
}

public sealed class SwitchTriggerZoneMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TriggerSwitch;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public SwitchTriggerZoneMode ActivationMode
    {
        get => (SwitchTriggerZoneMode)InternalData[1];
        set => InternalData[1] = (int)value;
    }

    public bool DestroysBeemerangWhileInsideAndDeactivated
    {
        get => InternalData[2] == 1;
        set => InternalData[2] = value ? 1 : 0;
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

    internal SwitchTriggerZoneMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, 1, 0]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(0, 3 - InternalData.Count));

        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }
    }
}