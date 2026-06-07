using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

public sealed class TimerSwitchMapEntityLeaf : MapEntityLeaf
{
    internal TimerSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Switch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public int TimerInFramesBeforeAutomaticTurnOff
    {
        get => InternalData[2].Value;
        set => InternalData[2].Value = value;
    }

    public bool CanOnlyBeActuatedWithHornSlash
    {
        get => InternalData[4].Value == 1;
        set => InternalData[4].Value = value ? 1 : 0;
    }

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Branch<AnimIdLeaf>? AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public int? RegionalFlagIdSetWhenActuated
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(0), new(30), new(0), new(0)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SwitchCrystal - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
        InternalBoxColCenter = Vector3.up * 0.5f;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5 - InternalData.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        if (InternalAnimIdOrItemId > 0)
            AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}