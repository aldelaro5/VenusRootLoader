using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.ActivatorZones;

public abstract class ActivatorZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    protected ActivatorZoneMapEntityLeaf(int gameId, string creatorId, string namedId) : base(
        gameId,
        creatorId,
        namedId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TriggerSwitch;

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? FlagSetToTrueWhenTriggered
    {
        get;
        set
        {
            InternalActivationFlagId = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    protected void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting()
    {
        if (InternalData.Count < 3)
        {
            int count = InternalData.Count;
            for (int i = 0; i < 3 - count; i++)
                InternalData.Add(new Ref<int>(0));
        }

        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();
            FlagSetToTrueWhenTriggered = new(flagsRegistry.GetByGameId(InternalActivationFlagId));
        }
    }
}