using VenusRootLoader.Api.Common;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

// TODO: Merge with the multi flags one later with a patch to fix its problems
public sealed class AndGateOnSingleFlagMapEntity : MapEntity
{
    internal AndGateOnSingleFlagMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

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
        InternalData.AddRange([0, -1]);
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
        InternalActivationFlagId = 0;
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