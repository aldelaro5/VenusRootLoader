using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

// TODO: Consider moving this to a leaf, collector would hardcode the ones that exists
public enum PressurePlateType
{
    RegularWoodenPlate,
    DarkGreenWoodenPlate,
    AncientPressurePlate,
    TestButton
}

public sealed class PressurePlateMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
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
                    InternalVectorData[0] = new(0f, -0.2f, 0f);
                    InternalBoxColSize = new(2f, 1f, 2f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.DarkGreenWoodenPlate:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.WoodenPPlate2 - 1;
                    InternalVectorData[0] = new(0f, -0.2f, 0f);
                    InternalBoxColSize = new(2f, 1f, 2f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.AncientPressurePlate:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPressurePlate - 1;
                    InternalVectorData[0] = new(0f, 0f, -0.004f);
                    InternalBoxColSize = new(3f, 1f, 3f);
                    InternalBoxColCenter = new(0f, 1f, 0f);
                    break;
                case PressurePlateType.TestButton:
                    InternalAnimIdOrItemId = (int)MainManager.AnimIDs.TestButton - 1;
                    InternalVectorData[0] = new(0f, -0.4f, 0f);
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

    public bool CanBeActuatedByPlayerCollider
    {
        get => InternalData[0] == 1;
        set => InternalData[0] = value ? 1 : 0;
    }

    public bool CanBeActuatedByEntityIceCube
    {
        get => InternalData[1] == 1;
        set => InternalData[1] = value ? 1 : 0;
    }

    public Branch<EventLeaf>? EventToTriggerOnFirstActuation
    {
        get;
        set
        {
            InternalData[2] = value?.GameId ?? -1;
            field = value;
        }
    }

    public Branch<FlagLeaf>? OneShotActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal PressurePlateMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([1, 1, -1]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.WoodenPPlate - 1;
        InternalVectorData.AddRange([new(0f, -0.2f, 0f)]);
        InternalActivationFlagId = -1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 1f, 2f);
        InternalBoxColCenter = new(0f, 1f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count > 2 && InternalData[2] >= 0)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
            EventToTriggerOnFirstActuation = new(eventsRegistry.LeavesByGameIds[InternalData[2]]);
        }

        if (InternalActivationFlagId >= 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            OneShotActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
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