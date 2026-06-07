using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

// TODO: Merge with the multi flags one later with a patch to fix its problems
public sealed class AndBlockOnSingleFlagMapEntityLeaf : AndBlockMapEntityLeaf
{
    internal AndBlockOnSingleFlagMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public NegatableFlag FlagInput
    {
        get;
        set
        {
            InternalActivationFlagId = value.EffectiveValue;
            field = value;
        }
    } = null!;

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(-1)]);
        InternalActivationFlagId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        FlagInput = new()
        {
            Flag = new(flagsRegistry.LeavesByGameIds[Math.Abs(InternalActivationFlagId)]),
            IsValueNegated = InternalActivationFlagId < 0
        };
    }
}