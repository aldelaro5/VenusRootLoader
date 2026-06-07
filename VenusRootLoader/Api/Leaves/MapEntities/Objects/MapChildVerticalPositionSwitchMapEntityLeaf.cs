using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: it's possible this works with other animid, recheck later
public enum MapChildVerticalPositionSwitchKind
{
    BigCrystalSwitch = 54,
    SwitchCrystal = 36
}

public sealed class MapChildVerticalPositionSwitchMapEntityLeaf : MapEntityLeaf
{
    internal MapChildVerticalPositionSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.WaterSwitch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public MapChildVerticalPositionSwitchKind SwitchKind
    {
        get => (MapChildVerticalPositionSwitchKind)InternalAnimIdOrItemId;
        set => InternalAnimIdOrItemId = (int)value;
    }

    public int MapChildIndexToMove { get => InternalData[0].Value; set => InternalData[0].Value = value; }

    public bool CanOnlyBeToggledUsingHornSlashAndHornDash
    {
        get => InternalData[4].Value == 1;
        set => InternalData[4].Value = value ? 1 : 0;
    }

    public float FramesDurationForFullMovement
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float VerticalMovementUpperBound
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float VerticalMovementLowerBound
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
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
        InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5));
        InternalVectorData.Add(new(new(180f, -1f, 1f)));
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