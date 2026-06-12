using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

// TODO: Merge with the multi flags one later with a patch to fix its problems
public sealed class AndGateOnSingleFlagMapEntityLeaf : AndGateMapEntityLeaf
{
    internal AndGateOnSingleFlagMapEntityLeaf(int gameId, string namedId, string creatorId)
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

    internal void InitializeFromNew(NegatableFlag flagInput)
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(-1)]);
        FlagInput = flagInput;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        FlagInput = new()
        {
            Flag = new(flagsRegistry.LeavesByGameIds[Math.Abs(InternalActivationFlagId)]),
            IsValueNegated = InternalActivationFlagId < 0
        };
    }
}