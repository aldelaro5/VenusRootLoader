using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

// TODO: it's possible this works with other animid, recheck later
public enum MapChildVerticalPositionSwitchKind
{
    BigCrystalSwitch = 54,
    SwitchCrystal = 36
}

public sealed class MapChildVerticalPositionSwitchMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.WaterSwitch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public MapChildVerticalPositionSwitchKind SwitchKind
    {
        get => (MapChildVerticalPositionSwitchKind)InternalAnimIdOrItemId;
        set => InternalAnimIdOrItemId = (int)value;
    }

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int MapChildIndexToMove { get => InternalData[0]; set => InternalData[0] = value; }

    public bool CanOnlyBeToggledUsingHornSlashAndHornDash
    {
        get => InternalData[4] == 1;
        set => InternalData[4] = value ? 1 : 0;
    }

    public float FramesDurationForFullMovement
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float VerticalMovementUpperBound
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public float VerticalMovementLowerBound
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, InternalVectorData[0].y, value);
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

    internal MapChildVerticalPositionSwitchMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, 0, 0, 0]);
        InternalVectorData.Add(new(180f, -1f, 1f));
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