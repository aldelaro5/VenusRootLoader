using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public sealed class AndGateOnEntitiesLeafActivationMapEntityLeaf : MapEntityLeaf
{
    internal AndGateOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _entityActivationsInput = new(InternalData, 1, x => x.Ref);
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    private readonly ListRefWrapper<NegatableMapEntityActivation, int> _entityActivationsInput;
    public IList<NegatableMapEntityActivation> EntityActivationsInput => _entityActivationsInput;

    public Branch<EventLeaf>? OneShotEventOutputOverride
    {
        get;
        set
        {
            InternalData[0].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
        InternalActivationFlagId = -1;
        InternalData.AddRange([new(-1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData[0].Value > -1)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
            OneShotEventOutputOverride = new(eventsRegistry.LeavesByGameIds[InternalData[0].Value]);
        }

        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
        _entityActivationsInput.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new NegatableMapEntityActivation
                {
                    MapEntity = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(x.Value)],
                    IsActivationValueNegated = x.Value < 0
                })
                .ToList());
    }
}