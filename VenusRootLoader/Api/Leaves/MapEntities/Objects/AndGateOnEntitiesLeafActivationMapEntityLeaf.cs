using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class AndGateOnEntitiesLeafActivationMapEntityLeaf : MapEntityLeaf
{
    internal AndGateOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public ReadOnlyCollection<NegatableMapEntityActivation> EntityActivationsInput { get; private set; } =
        new List<NegatableMapEntityActivation>().AsReadOnly();

    public Branch<EventLeaf>? OneShotEventOutputOverride
    {
        get;
        set
        {
            InternalData[0] = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
        InternalActivationFlagId = -1;
        InternalData.AddRange([-1]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData[0] > -1)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
            OneShotEventOutputOverride = new(eventsRegistry.LeavesByGameIds[InternalData[0]]);
        }

        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
        List<NegatableMapEntityActivation> entityActivationInputs = new();
        for (int i = 1; i < InternalData.Count; i++)
        {
            int value = InternalData[i];
            entityActivationInputs.Add(
                new()
                {
                    MapEntityLeaf = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(value)],
                    IsActivationValueNegated = value < 0
                });
        }

        ChangeEntitiesActivationInput(entityActivationInputs);
    }

    public void ChangeEntitiesActivationInput(List<NegatableMapEntityActivation> entityActivationsInput)
    {
        List<NegatableMapEntityActivation> incorrectEntities = entityActivationsInput
            .Where(e => e.MapEntityLeaf.Map != Map)
            .ToList();
        if (incorrectEntities.Count > 0)
        {
            IEnumerable<string> badEntityNames = incorrectEntities.Select(e => e.MapEntityLeaf.BaseGameObjectName);
            ThrowHelper.ThrowArgumentOutOfRangeException(
                nameof(entityActivationsInput),
                $"The following entities are not present in the {Map.NamedId} map which is required: " +
                $"{string.Join(", ", badEntityNames)}");
        }

        InternalData.RemoveRange(1, InternalData.Count - 1);

        foreach (NegatableMapEntityActivation negatableMapEntityActivation in entityActivationsInput)
            InternalData.Add(negatableMapEntityActivation.EffectiveValue);

        EntityActivationsInput = entityActivationsInput.AsReadOnly();
    }
}