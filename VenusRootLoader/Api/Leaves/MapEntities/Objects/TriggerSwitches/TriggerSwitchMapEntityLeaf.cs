using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.TriggerSwitches;

public abstract class TriggerSwitchMapEntityLeaf : ObjectMapEntityLeaf
{
    protected TriggerSwitchMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TriggerSwitch;

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
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 3 - InternalData.Count));

        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }
    }
}