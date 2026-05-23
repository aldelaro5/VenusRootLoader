using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Common;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class AndBlockOnEntitiesActivationMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public ReadOnlyCollection<NegatableMapEntityActivation> EntityActivationsInput { get; private set; } =
        new List<NegatableMapEntityActivation>().AsReadOnly();

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Branch<AnimIdLeaf>? AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 LocalPositionWhenActuatedAfterLerp
    {
        get => InternalVectorData[0];
        set => InternalVectorData[0] = value;
    }

    public float LocalPositionLerpFactorWhenActuated
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public Vector3? EntityStartScale
    {
        get => InternalVectorData[2].magnitude <= 0.1f ? null : InternalVectorData[2];
        set => InternalVectorData[2] = value ?? Vector3.zero;
    }

    internal AndBlockOnEntitiesActivationMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1]);
        InternalVectorData.AddRange([Vector3.down * 6f, Vector3.right * 0.1f, Vector3.zero]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PrisonGate - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalVectorData.Count));

        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
        ILeavesRegistry<AnimIdLeaf> animidRegistry = registryResolver.Resolve<AnimIdLeaf>();

        List<NegatableMapEntityActivation> entityActivationInputs = new();
        for (int i = 1; i < InternalData.Count; i++)
        {
            int value = InternalData[i];
            entityActivationInputs.Add(
                new()
                {
                    MapEntity = map.Entities.Single(e => e.Id == Math.Abs(value)),
                    IsActivationValueNegated = value < 0
                });
        }

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animidRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        ChangeEntitiesActivationInput(entityActivationInputs);
    }

    public void ChangeEntitiesActivationInput(List<NegatableMapEntityActivation> entityActivationsInput)
    {
        List<NegatableMapEntityActivation> incorrectEntities = entityActivationsInput.Where(e => e.MapEntity.Map != Map)
            .ToList();
        if (incorrectEntities.Count > 0)
        {
            IEnumerable<string> badEntityNames = incorrectEntities.Select(e => e.MapEntity.Name);
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