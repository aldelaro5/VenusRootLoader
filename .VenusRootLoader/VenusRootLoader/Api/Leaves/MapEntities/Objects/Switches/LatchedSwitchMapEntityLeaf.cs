using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

// Remember to point out in the xmldocs that the regional gets set upon activation
public sealed class LatchedSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal LatchedSwitchMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<FlagLeaf> LatchHoldFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<FlagLeaf> latchHoldFlag)
    {
        base.InitializeFromNew(startingPosition, animId);
        for (int i = 0; i < 5; i++)
            InternalData.Add(new Ref<int>(0));
        LatchHoldFlag = latchHoldFlag;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();
        LatchHoldFlag = new(flagsRegistry.GetByGameId(InternalActivationFlagId));
    }
}