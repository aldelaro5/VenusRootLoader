using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

// TODO: Remember to point out in the xmldocs that the regional gets set upon activation
public sealed class LatchedSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal LatchedSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<FlagLeaf> LatchHoldFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value.GameId;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<FlagLeaf> latchHoldFlag,
        Branch<AnimIdLeaf>? animId)
    {
        base.InitializeFromNew(startingPosition, animId);
        for (int i = 0; i < 5; i++)
            InternalData.Add(new Ref<int>(0));
        LatchHoldFlag = latchHoldFlag;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        LatchHoldFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}