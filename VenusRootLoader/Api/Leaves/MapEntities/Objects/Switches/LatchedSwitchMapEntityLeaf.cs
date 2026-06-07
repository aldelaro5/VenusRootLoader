using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

// TODO: Remember to point out in the xmldocs that the regional gets set upon actuation
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

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5));
        InternalActivationFlagId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        LatchHoldFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}