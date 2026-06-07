using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public sealed class AndGateOnFlagsMapEntityLeaf : MapEntityLeaf
{
    internal AndGateOnFlagsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _flagsInput = new(InternalData, 1, x => new(-x.GameId));
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    private readonly ListRefWrapper<Branch<FlagLeaf>, int> _flagsInput;
    public IList<Branch<FlagLeaf>> FlagsInput => _flagsInput;

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
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