using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public sealed class AndGateOnFlagsMapEntityLeaf : AndGateMapEntityLeaf
{
    internal AndGateOnFlagsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _flagsInput = new(InternalData, 1, x => new(-x.GameId));
    }

    private readonly ListRefWrapper<Branch<FlagLeaf>, int> _flagsInput;
    public IList<Branch<FlagLeaf>> FlagsInput => _flagsInput;

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalActivationFlagId = -1;
        InternalData.AddRange([new(-2)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        _flagsInput.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new Branch<FlagLeaf>(flagsRegistry.LeavesByGameIds[Math.Abs(x.Value)]))
                .ToList());
    }
}