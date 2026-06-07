using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

public sealed class LinkableToggleSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal LinkableToggleSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public int CooldownInFramesBeforeToggleableAgain
    {
        get => InternalData[2].Value;
        set => InternalData[2].Value = value;
    }

    public Branch<FlagLeaf>? LinkFlag
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
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(1), new(0), new(0), new(0)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        if (InternalActivationFlagId > 0)
            LinkFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}