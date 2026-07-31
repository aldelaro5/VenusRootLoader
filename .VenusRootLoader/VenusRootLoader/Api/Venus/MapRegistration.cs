// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Unity.AssetLoading;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.Runtime.Enums;
using VenusRootLoader.Unity.Runtime.ScriptableObjects;
using MapAutoEvent = VenusRootLoader.Unity.Runtime.ScriptableObjects.MapAutoEvent;
using MapInside = VenusRootLoader.Unity.Runtime.ScriptableObjects.MapInside;
using MapMusicSelectionCondition = VenusRootLoader.Unity.Runtime.ScriptableObjects.MapMusicSelectionCondition;

namespace VenusRootLoader.Api;

public partial class Venus
{
    public MapLeaf RegisterMap(
        string namedId,
        IAssetLoader<GameObject> prefabLoader,
        Branch<AreaLeaf> area,
        Branch<DialogueLeaf> spyDialogue,
        Branch<MusicLeaf>? defaultMapMusic)
    {
        MapLeaf mapLeaf = RegistryResolver.Resolve<MapLeaf>().RegisterNew(BudId, namedId);
        mapLeaf.PrefabLoader = prefabLoader;
        mapLeaf.Area = area;
        mapLeaf.SpyDialogue = spyDialogue;
        mapLeaf.AddMusicToMap(defaultMapMusic);
        mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
            IdSequenceDirection.Increment);
        return mapLeaf;
    }

    public MapLeaf RegisterMapFromScriptableObjectInAssetBundle(
        string namedId,
        AssetBundle assetBundle,
        string pathToScriptableObjectInBundle)
    {
        MapLeafScriptableObject scriptableObject =
            assetBundle.LoadAsset<MapLeafScriptableObject>(pathToScriptableObjectInBundle);
        if (scriptableObject == null)
        {
            ThrowHelper.ThrowArgumentException(
                $"No such {nameof(MapLeafScriptableObject)} exists in this AssetBundle at path {pathToScriptableObjectInBundle}");
        }

        MapLeaf mapLeaf = RegistryResolver.Resolve<MapLeaf>().RegisterNew(BudId, namedId);
        mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.PrefabLoader = new AssetLoaderFromBundle<GameObject>(assetBundle, scriptableObject.Prefab);

        mapLeaf.Area = RegistryResolver.Resolve<AreaLeaf>()
            .LeavesByEffectiveIds[GetEffectiveIdFromScriptableObjectBranch(scriptableObject.Area, BudId)];

        mapLeaf.DefaultCameraPositionOffsetFromTargetOverride =
            scriptableObject.DefaultCameraPositionOffsetFromTargetOverrideHasValue
                ? scriptableObject.DefaultCameraPositionOffsetFromTargetOverride
                : null;

        mapLeaf.DefaultCameraAnglesOffsetFromTargetOverride =
            scriptableObject.DefaultCameraAnglesOffsetFromTargetOverrideHasValue
                ? scriptableObject.DefaultCameraAnglesOffsetFromTargetOverride
                : null;

        mapLeaf.DefaultCameraLowerBounds = scriptableObject.DefaultCameraLowerBounds;
        mapLeaf.DefaultCameraUpperBounds = scriptableObject.DefaultCameraUpperBounds;

        if (scriptableObject.CameraMovesAroundCircleHasValue)
        {
            mapLeaf.CameraMoveAroundCircleConfiguration = new()
            {
                InitialCircleCenter = scriptableObject.CameraMovesAroundCircle.InitialCircleCenter,
                CameraFollowsTargetInYAxis = scriptableObject.CameraMovesAroundCircle.CameraFollowsTargetInYAxis,
                CameraMaxRadiusFromCenterPointAllowed =
                    scriptableObject.CameraMovesAroundCircle.CameraMaxRadiusFromCenterPointAllowedHasValue
                        ? scriptableObject.CameraMovesAroundCircle.CameraMaxRadiusFromCenterPointAllowed
                        : null
            };
        }
        else
        {
            mapLeaf.CameraMoveAroundCircleConfiguration = null;
        }

        mapLeaf.InitialFogEndDistance = scriptableObject.InitialFogEndDistance;
        mapLeaf.InitialFogColor = scriptableObject.InitialFogColor;
        mapLeaf.HasSunRaysTopRightScreenEffect = scriptableObject.HasSunRaysTopRightScreenEffect;
        mapLeaf.SkyboxMaterial = scriptableObject.SkyboxMaterial;
        mapLeaf.InitialAmbientLightColor = scriptableObject.InitialAmbientLightColor;
        mapLeaf.WindIntensity = scriptableObject.WindIntensity;
        mapLeaf.ForceAllFadersToFadeInsteadOfCulling = scriptableObject.ForceAllFadersToFadeInsteadOfCulling;
        mapLeaf.AllFadersFadingTint = scriptableObject.AllFadersFadingTint;

        mapLeaf.DefaultBattleMap = (MainManager.BattleMaps)scriptableObject.DefaultBattleMap;
        mapLeaf.BattleTransition = (MapControl.BattleLeafType)scriptableObject.BattleTransition;
        mapLeaf.ExpMultiplier = scriptableObject.ExpMultiplier;
        mapLeaf.DefaultBattleTransitionLeavesColor = scriptableObject.DefaultBattleTransitionLeavesColor;
        mapLeaf.DisableMusicChangeWhenEnteringBattle = scriptableObject.DisableMusicChangeWhenEnteringBattle;

        foreach (Branch music in scriptableObject.MusicsAvailable)
        {
            string musicId = GetEffectiveIdFromScriptableObjectBranch(music, BudId);
            Branch<MusicLeaf>? musicBranch = musicId != nameof(Musics.Silence)
                ? RegistryResolver.Resolve<MusicLeaf>().LeavesByEffectiveIds[musicId]
                : (Branch<MusicLeaf>?)null;
            mapLeaf.AddMusicToMap(musicBranch);
        }

        mapLeaf.KeepsExistingMusicPlayingOnLoad = scriptableObject.KeepsExistingMusicPlayingOnLoad;
        foreach (MapMusicSelectionCondition musicSelectionCondition in scriptableObject.MusicSelectionConditions)
        {
            mapLeaf.MusicSelectionConditions.Add(
                new()
                {
                    RequiredFlag = musicSelectionCondition.RequiredFlagHasValue
                        ? RegistryResolver.Resolve<FlagLeaf>().LeavesByEffectiveIds[
                            GetEffectiveIdFromScriptableObjectBranch(musicSelectionCondition.RequiredFlag, BudId)]
                        : (Branch<FlagLeaf>?)null,
                    MapMusic = mapLeaf.MusicsAvailable[musicSelectionCondition.MusicIndexInMap]
                });
        }

        foreach (MapInside mapInside in scriptableObject.Insides)
        {
            mapLeaf.Insides.Add(
                new()
                {
                    GameObjectPathInPrefab = mapInside.Transform,
                    TransitionWhenEnteringOrExiting = (MapControl.InsideType)mapInside.TransitionWhenEnteringOrExiting
                });
        }

        mapLeaf.ForceRestoreCameraWhenExitingAnyInsideTransitionZone =
            scriptableObject.ForceRestoreCameraWhenExitingAnyInsideTransitionZone;
        mapLeaf.DisablesInsideWhenCurrentInsideIsDifferent =
            scriptableObject.DisablesInsideWhenCurrentInsideIsDifferent;
        mapLeaf.SetCameraTargetToCurrentInsideWhileInside = scriptableObject.SetCameraTargetToCurrentInsideWhileInside;
        mapLeaf.FadingSpeedWhenEnteringOrExitingAnInside = scriptableObject.FadingSpeedWhenEnteringOrExitingAnInside;

        mapLeaf.SpyDialogue = scriptableObject.SpyDialogue.DialogueKind == MapDialogueKind.Common
            ? RegistryResolver.Resolve<CommonDialogueLeaf>().LeavesByEffectiveIds[
                GetEffectiveIdFromScriptableObjectBranch(scriptableObject.SpyDialogue.Dialogue, BudId)]
            : CreateMapDialogueBranch(scriptableObject.SpyDialogue.Dialogue);

        foreach (Branch branch in scriptableObject.FollowerAnimIdsAllowed)
        {
            Branch<AnimIdLeaf> animId = RegistryResolver.Resolve<AnimIdLeaf>()
                .LeavesByEffectiveIds[GetEffectiveIdFromScriptableObjectBranch(branch, BudId)];
            mapLeaf.FollowerAnimIdsAllowed.Add(animId);
        }

        mapLeaf.MaximumYFollowerDistanceBeforeTeleport = scriptableObject.MaximumYFollowerDistanceBeforeTeleport;

        mapLeaf.AllEntitiesYPositionLowerBoundLimitBeforeRespawn =
            scriptableObject.AllEntitiesYPositionLowerBoundLimitBeforeRespawn;
        mapLeaf.IsFrozenMap = scriptableObject.IsFrozenMap;
        mapLeaf.MapEntitiesHaveRestrictedActiveRange = scriptableObject.MapEntitiesHaveRestrictedActiveRange;
        mapLeaf.MapEntitiesAndEmoticonsAreActiveWhenOutOfRange =
            scriptableObject.MapEntitiesAndEmoticonsAreActiveWhenOutOfRange;
        mapLeaf.MainMapTransformOverridePrefabPath = !string.IsNullOrEmpty(scriptableObject.MainMapTransformOverride)
            ? scriptableObject.MainMapTransformOverride
            : null;

        foreach (Branch branch in scriptableObject.DetectableDiscoveriesByDetectorMedal)
        {
            Branch<DiscoveryLeaf> discoveryLeaf = RegistryResolver.Resolve<DiscoveryLeaf>()
                .LeavesByEffectiveIds[GetEffectiveIdFromScriptableObjectBranch(branch, BudId)];
            mapLeaf.DetectableDiscoveriesByDetectorMedal.Add(discoveryLeaf);
        }

        mapLeaf.MapWhoProvidesEntitiesAndDialogues = scriptableObject.MapWhoProvidesEntitiesAndDialoguesHasValue
            ? RegistryResolver.Resolve<MapLeaf>()
                .LeavesByEffectiveIds[GetEffectiveIdFromScriptableObjectBranch(
                    scriptableObject.RedirectsEntitiesAndDialoguesToAnotherMap,
                    BudId)]
            : (Branch<MapLeaf>?)null;
        mapLeaf.DisallowAntCompassUsage = scriptableObject.DisallowAntCompassUsage;

        foreach (MapAutoEvent mapAutoEvent in scriptableObject.AutomaticallyTriggeredEventsAfterLoad)
        {
            mapLeaf.AutomaticallyTriggeredEventsAfterLoad.Add(
                new()
                {
                    AlreadyTriggeredFlag = RegistryResolver.Resolve<FlagLeaf>().LeavesByEffectiveIds[
                        GetEffectiveIdFromScriptableObjectBranch(mapAutoEvent.AlreadyTriggeredFlag, BudId)],
                    EventToTriggerWhenFlagIsFalse = RegistryResolver.Resolve<EventLeaf>().LeavesByEffectiveIds[
                        GetEffectiveIdFromScriptableObjectBranch(mapAutoEvent.EventToTriggerWhenFlagIsFalse, BudId)],
                });
        }

        foreach (MapEventTransform mapEventTransform in scriptableObject.EventsTransform)
            mapLeaf.EventsGameObjectPrefabPaths.Add(mapEventTransform.Transform);

        return mapLeaf;
    }

    // TODO: Need branches to lazily resolve leaves
    private Branch<DialogueLeaf> CreateMapDialogueBranch(Branch dialogueBranch)
    {
        string creatorId = dialogueBranch.CreatorKind == BranchCreatorKind.Custom
            ? dialogueBranch.CustomCreatorId
            : BudId;
        MapDialogueLeaf mapDialogueLeaf = new(-1, dialogueBranch.NamedId, creatorId);
        return mapDialogueLeaf;
    }

    private static string GetEffectiveIdFromScriptableObjectBranch(Branch scriptableObjectBranch, string mapCreatorId)
    {
        return scriptableObjectBranch.CreatorKind switch
        {
            BranchCreatorKind.BaseGame => scriptableObjectBranch.NamedId,
            BranchCreatorKind.SameAsThisAsset =>
                $"{mapCreatorId}{Constants.LeafEffectiveIdSeparator}{scriptableObjectBranch.NamedId}",
            BranchCreatorKind.Custom =>
                $"{scriptableObjectBranch.CustomCreatorId}{Constants.LeafEffectiveIdSeparator}{scriptableObjectBranch.NamedId}",
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(Branch.CreatorKind))
        };
    }
}