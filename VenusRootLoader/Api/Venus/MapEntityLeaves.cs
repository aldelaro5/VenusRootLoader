using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Leaves.MapEntities.Npcs;
using VenusRootLoader.Api.Leaves.MapEntities.Objects;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;
using VenusRootLoader.SourceGenerators;

// ReSharper disable CheckNamespace

namespace VenusRootLoader.Api;

public partial class Venus
{
    [MapEntityRegisterMethod]
    private TMapEntity RegisterMapEntity<TMapEntity>(string namedId, MapLeaf map)
        where TMapEntity : MapEntityLeaf
    {
        TMapEntity mapEntityLeaf = map.EntitiesRegistry.RegisterNew<TMapEntity>(namedId, BudId);
        mapEntityLeaf.BaseGameObjectName = namedId;
        mapEntityLeaf.Map = map;
        return mapEntityLeaf;
    }

    // The following overloads are written by hand since they need to use the resolver to access default values

    public ItemsStorageNpcMapEntityLeaf RegisterItemsStorageNpcMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.MessengerAnt)];
        return RegisterItemsStorageNpcMapEntity(namedId, map, startingPosition, animId);
    }

    public VenusHealingNpcMapEntityLeaf RegisterVenusHealingNpcMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.AngryPlant)];
        return RegisterVenusHealingNpcMapEntity(namedId, map, startingPosition, animId);
    }

    public AndBlockOnEntitiesLeafActivationMapEntityLeaf RegisterAndBlockOnEntitiesLeafActivationMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<NegatableMapEntityActivation> entityActivationsInputs)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.PrisonGate)];
        return RegisterAndBlockOnEntitiesLeafActivationMapEntity(
            namedId,
            map,
            startingPosition,
            animId,
            entityActivationsInputs);
    }

    public AndBlockOnFlagsMapEntityLeaf RegisterAndBlockOnFlagsMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<Branch<FlagLeaf>> flagInputs)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.PrisonGate)];
        return RegisterAndBlockOnFlagsMapEntity(namedId, map, startingPosition, animId, flagInputs);
    }

    public AndBlockOnSingleFlagMapEntityLeaf RegisterAndBlockOnSingleFlagMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        NegatableFlag flagInput)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.PrisonGate)];
        return RegisterAndBlockOnSingleFlagMapEntity(namedId, map, startingPosition, animId, flagInput);
    }

    public MovingPlatformAlongLerpMapEntityLeaf RegisterMovingPlatformAlongLerpMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove,
        UnityEngine.Vector3 fromPosition,
        UnityEngine.Vector3 toPosition)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.AncientPlatform)];
        return RegisterMovingPlatformAlongLerpMapEntity(
            namedId,
            map,
            startingPosition,
            animId,
            requiredEntityActivationsToMove,
            fromPosition,
            toPosition);
    }

    public MovingPlatformAlongPathMapEntityLeaf RegisterMovingPlatformAlongPathMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove,
        IList<UnityEngine.Vector3> movementPathNodePositions)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.AncientPlatform)];
        return RegisterMovingPlatformAlongPathMapEntity(
            namedId,
            map,
            startingPosition,
            animId,
            requiredEntityActivationsToMove,
            movementPathNodePositions);
    }

    public EventTriggerSwitchMapEntityLeaf RegisterEventTriggerSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        Branch<EventLeaf> eventToStartWhenToggled)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.SwitchCrystal)];
        return RegisterEventTriggerSwitchMapEntity(namedId, map, startingPosition, eventToStartWhenToggled, animId);
    }

    public LatchedSwitchMapEntityLeaf RegisterLatchedSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        Branch<FlagLeaf> latchHoldFlag)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.SwitchCrystal)];
        return RegisterLatchedSwitchMapEntity(namedId, map, startingPosition, latchHoldFlag, animId);
    }

    public LinkableToggleSwitchMapEntityLeaf RegisterLinkableToggleSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        Branch<FlagLeaf>? linkFlag)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.SwitchCrystal)];
        return RegisterLinkableToggleSwitchMapEntity(namedId, map, startingPosition, linkFlag, animId);
    }

    public TimerSwitchMapEntityLeaf RegisterTimerSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        int timerInFramesBeforeAutomaticTurnOff)
    {
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>()
            .LeavesByNamedIds[nameof(MainManager.AnimIDs.SwitchCrystal)];
        return RegisterTimerSwitchMapEntity(
            namedId,
            map,
            startingPosition,
            timerInFramesBeforeAutomaticTurnOff,
            animId);
    }
}