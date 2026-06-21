using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class PressurePlateMapEntityLeaf : ObjectMapEntityLeaf
{
    internal PressurePlateMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PressurePlate;

    public PressurePlateType PressurePlateType
    {
        get;
        set
        {
            switch (value)
            {
                case PressurePlateType.RegularWoodenPlate:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.WoodenPPlate - 1;
                    InternalVectorData[0].Value = new(0f, -0.2f, 0f);
                    InternalBoxColSize = new(2f, 1f, 2f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.DarkGreenWoodenPlate:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.WoodenPPlate2 - 1;
                    InternalVectorData[0].Value = new(0f, -0.2f, 0f);
                    InternalBoxColSize = new(2f, 1f, 2f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.AncientPressurePlate:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPressurePlate - 1;
                    InternalVectorData[0].Value = new(0f, 0f, -0.004f);
                    InternalBoxColSize = new(3f, 1f, 3f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.TestButton:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.TestButton - 1;
                    InternalVectorData[0].Value = new(0f, -0.4f, 0f);
                    InternalBoxColSize = Vector3.one;
                    InternalBoxColCenter = Vector3.zero;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                    break;
            }

            field = value;
        }
    }

    public bool CanBeActivatedByPlayerCollider
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public bool CanBeActivatedByEntityIceCube
    {
        get => InternalData[1].Value == 1;
        set => InternalData[1].Value = value ? 1 : 0;
    }

    public Branch<EventLeaf>? EventToTriggerOnFirstActivation
    {
        get;
        set
        {
            InternalData[2].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagActivationOverrideWhenTrue
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, PressurePlateType pressurePlateType)
    {
        InternalData.AddRange([new(1), new(1), new(-1)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.WoodenPPlate - 1;
        InternalVectorData.AddRange([new(new(0f, -0.2f, 0f))]);
        InternalActivationFlagId = -1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 1f, 2f);
        InternalBoxColCenter = new(0f, 1f, 0f);
        PressurePlateType = pressurePlateType;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
        {
            for (int i = 0; i < 3 - InternalData.Count; i++)
                InternalData.Add(new Ref<int>(-1));
        }

        if (InternalData[2].Value >= 0)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
            EventToTriggerOnFirstActivation = new(eventsRegistry.LeavesByGameIds[InternalData[2].Value]);
        }

        if (InternalActivationFlagId >= 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            FlagActivationOverrideWhenTrue = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }

        PressurePlateType = (MainManager.AnimIDs)(InternalAnimIdOrItemId + 1) switch
        {
            MainManager.AnimIDs.WoodenPPlate => PressurePlateType.RegularWoodenPlate,
            MainManager.AnimIDs.WoodenPPlate2 => PressurePlateType.DarkGreenWoodenPlate,
            MainManager.AnimIDs.AncientPressurePlate => PressurePlateType.AncientPressurePlate,
            MainManager.AnimIDs.TestButton => PressurePlateType.TestButton,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<PressurePlateType>(nameof(InternalAnimIdOrItemId))
        };
    }
}