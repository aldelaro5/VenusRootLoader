using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Leaves.MapEntities.Npcs;
using VenusRootLoader.Api.Leaves.MapEntities.Objects;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

// ReSharper disable CheckNamespace

namespace VenusRootLoader.Api;

public partial class Venus
{
    [MapEntityRegisterMethod]
    private TMapEntity RegisterMapEntity<TMapEntity>(string namedId, MapLeaf map)
        where TMapEntity : MapEntityLeaf
    {
        TMapEntity mapEntityLeaf = map.EntitiesRegistry.RegisterNew<TMapEntity>(BudId, namedId);
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
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.MessengerAnt));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterItemsStorageNpcMapEntity(namedId, map, startingPosition, animId);
    }

    public VenusHealingNpcMapEntityLeaf RegisterVenusHealingNpcMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.AngryPlant));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterVenusHealingNpcMapEntity(namedId, map, startingPosition, animId);
    }

    public AndBlockOnEntitiesLeafActivationMapEntityLeaf RegisterAndBlockOnEntitiesLeafActivationMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<NegatableMapEntityActivation> entityActivationsInputs)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.PrisonGate));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
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
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.PrisonGate));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterAndBlockOnFlagsMapEntity(namedId, map, startingPosition, animId, flagInputs);
    }

    public AndBlockOnSingleFlagMapEntityLeaf RegisterAndBlockOnSingleFlagMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        NegatableFlag flagInput)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.PrisonGate));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterAndBlockOnSingleFlagMapEntity(namedId, map, startingPosition, animId, flagInput);
    }

    public MovingPlatformAlongLerpMapEntityLeaf RegisterMovingPlatformAlongLerpMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove,
        UnityEngine.Vector3 toPosition)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.AncientPlatform));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterMovingPlatformAlongLerpMapEntity(
            namedId,
            map,
            startingPosition,
            animId,
            requiredEntityActivationsToMove,
            toPosition);
    }

    public MovingPlatformAlongPathMapEntityLeaf RegisterMovingPlatformAlongPathMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove,
        IList<UnityEngine.Vector3> movementPathNodePositions)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.AncientPlatform));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
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
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.SwitchCrystal));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterEventTriggerSwitchMapEntity(namedId, map, startingPosition, animId, eventToStartWhenToggled);
    }

    public LatchedSwitchMapEntityLeaf RegisterLatchedSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        Branch<FlagLeaf> latchHoldFlag)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.SwitchCrystal));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterLatchedSwitchMapEntity(namedId, map, startingPosition, animId, latchHoldFlag);
    }

    public LinkableToggleSwitchMapEntityLeaf RegisterLinkableToggleSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        Branch<FlagLeaf>? linkFlag)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.SwitchCrystal));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterLinkableToggleSwitchMapEntity(namedId, map, startingPosition, animId, linkFlag);
    }

    public TimerSwitchMapEntityLeaf RegisterTimerSwitchMapEntity(
        string namedId,
        MapLeaf map,
        UnityEngine.Vector3 startingPosition,
        int timerInFramesBeforeAutomaticTurnOff)
    {
        string effectiveId = EffectiveLeafId.CreateBaseGameEffectiveId(nameof(MainManager.AnimIDs.SwitchCrystal));
        AnimIdLeaf animId = RegistryResolver.Resolve<AnimIdLeaf>().LeavesByEffectiveIds[effectiveId];
        return RegisterTimerSwitchMapEntity(
            namedId,
            map,
            startingPosition,
            animId,
            timerInFramesBeforeAutomaticTurnOff);
    }
}