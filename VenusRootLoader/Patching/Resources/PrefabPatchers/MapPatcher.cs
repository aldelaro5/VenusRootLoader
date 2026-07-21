using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

/// <summary>
/// An <see cref="IPrefabPatcher"/> that patches maps prefabs from the game.
/// </summary>
internal sealed class MapPatcher : IPrefabPatcher
{
    private readonly ILeavesRegistry<MapLeaf> _mapRegistry;

    public MapPatcher(string[] subPaths, ILeavesRegistry<MapLeaf> mapRegistry)
    {
        SubPaths = subPaths;
        _mapRegistry = mapRegistry;
    }

    public string[] SubPaths { get; }

    public Object PatchPrefab(string path, Object original)
    {
        int mapEffectiveIdStart = path.LastIndexOf('/') + 1;
        string mapEffectiveId = path[mapEffectiveIdStart..];
        MapLeaf map = _mapRegistry.LeavesByEffectiveIds[mapEffectiveId];
        if (original == null)
            return PrepareCustomMap(map);

        GameObject mapPrefab = (GameObject)original;
        MapControl mapControl = mapPrefab.GetComponent<MapControl>();
        PatchMapControl(map, mapPrefab, mapControl);
        mapPrefab.name = map.GameId.ToString();
        return original;
    }

    private static GameObject PrepareCustomMap(MapLeaf map)
    {
        GameObject mapPrefab = map.PrefabInstantiator!(map);
        MapControl mapControl = mapPrefab.AddComponent<MapControl>();
        PatchMapControl(map, mapPrefab, mapControl);
        mapPrefab.name = map.GameId.ToString();
        Object.Destroy(mapPrefab, 0.001f);
        return mapPrefab;
    }

    private static void PatchMapControl(MapLeaf map, GameObject mapPrefab, MapControl mapControl)
    {
        mapControl.mapid = (MainManager.Maps)map.GameId;
        mapControl.areaid = (MainManager.Areas)map.Area.GameId;

        mapControl.camoffset = map.DefaultCameraPositionOffsetFromTargetOverride ?? Vector3.zero;
        mapControl.camangle = map.DefaultCameraAnglesOffsetFromTargetOverride ?? Vector3.zero;
        mapControl.camlimitneg = map.DefaultCameraLowerBounds;
        mapControl.camlimitpos = map.DefaultCameraUpperBounds;
        mapControl.rotatecam = map.CameraMoveAroundCircleConfiguration is not null;
        mapControl.centralpoint = map.CameraMoveAroundCircleConfiguration?.InitialCircleCenter ?? Vector3.zero;
        mapControl.tieYtoplayer = map.CameraMoveAroundCircleConfiguration?.CameraFollowsTargetInYAxis ?? false;
        mapControl.tetherdistance =
            map.CameraMoveAroundCircleConfiguration?.CameraMaxRadiusFromCenterPointAllowed ?? -1f;

        mapControl.fogend = map.InitialFogEndDistance;
        mapControl.fogcolor = map.InitialFogColor;
        mapControl.screeneffect = map.HasSunRaysTopRightScreenEffect
            ? MapControl.ScreenEffects.SunRaysTopRight
            : MapControl.ScreenEffects.None;
        mapControl.skyboxmat = map.SkyboxMaterial;
        mapControl.globallight = map.InitialAmbientLightColor;
        mapControl.windintensity = map.WindIntensity;
        mapControl.faderchange = map.ForceAllFadersToFadeInsteadOfCulling;
        mapControl.skycolor = map.AllFadersFadingTint;

        mapControl.battlemap = map.DefaultBattleMap;
        mapControl.battleleaftype = map.BattleTransition;
        mapControl.expmulti = map.ExpMultiplier;
        mapControl.battleleafcolor = map.DefaultBattleTransitionLeavesColor;
        mapControl.nobattlemusic = map.DisableMusicChangeWhenEnteringBattle;

        mapControl.music = map.MusicsAvailable
            .Select(x => x.Music?.Leaf.Music ?? null)
            .ToArray();
        for (int i = 0; i < mapControl.music.Length; i++)
        {
            AudioClip audioClip = mapControl.music[i];
            if (audioClip != null)
                audioClip.name = map.MusicsAvailable[i].Music?.EffectiveId ?? "";
        }

        mapControl.keepmusic = map.KeepsExistingMusicPlayingOnLoad;
        mapControl.musicflags = map.MusicSelectionConditions
            .Select(x => new Vector2Int(x.RequiredFlag?.GameId ?? -1, x.MapMusic.MusicIdInMap))
            .ToArray();

        mapControl.insides = map.Insides
            .Select(x => mapPrefab.transform.Find(x.GameObjectPathInPrefab).gameObject)
            .ToArray();
        mapControl.insidetypes = map.Insides
            .Select(x => x.TransitionWhenEnteringOrExiting)
            .ToArray();
        mapControl.tieinsidedoorentities = map.ForceRestoreCameraWhenExitingAnyInsideTransitionZone;
        mapControl.hideinsides = map.DisablesInsideWhenCurrentInsideIsDifferent;
        mapControl.setinsidecenter = map.SetCameraTargetToCurrentInsideWhileInside;
        mapControl.fadingspeed = map.FadingSpeedWhenEnteringOrExitingAnInside;

        mapControl.tattleid = map.SpyDialogue.GameId;

        mapControl.canfollowID = map.FollowerAnimIdsAllowed
            .Select(x => x.GameId)
            .ToArray();
        mapControl.followerylimit = map.MaximumYFollowerDistanceBeforeTeleport;

        mapControl.ylimit = map.AllEntitiesYPositionLowerBoundLimitBeforeRespawn;
        mapControl.icemap = map.IsFrozenMap;
        mapControl.limitbehavior = map.MapEntitiesHaveRestrictedActiveRange;
        mapControl.keepobjectsactive = map.MapEntitiesAreKeptActive;

        if (map.MainMapTransformOverridePrefabPath is not null)
            mapControl.mainmesh = mapPrefab.transform.Find(map.MainMapTransformOverridePrefabPath);
        if (mapControl.mainmesh == null && mapPrefab.transform.childCount == 0)
            mapControl.mainmesh = mapPrefab.transform;
        mapControl.discoveryids = map.DetectableDiscoveriesByDetectorMedal
            .Select(x => x.GameId)
            .ToArray();
        mapControl.readdatafromothermap = (MainManager.Maps)(map.MapWhoProvidesEntitiesAndDialogues?.GameId ?? 0);
        mapControl.cantcompass = map.DisallowAntCompassUsage;
        mapControl.autoevent = map.AutomaticallyTriggeredEventsAfterLoad
            .Select(x => new Vector2(x.AlreadyTriggeredFlag.GameId, x.EventToTriggerWhenFlagIsFalse.GameId))
            .ToArray();
        mapControl.eventPointers = map.EventsGameObjectPrefabPaths
            .Select(x => mapPrefab.transform.Find(x).gameObject)
            .ToArray();

        SimulateUnityCollectionDeserialisationLogic(mapControl);
    }

    private static void SimulateUnityCollectionDeserialisationLogic(MapControl mapControl)
    {
        mapControl.insidetypes ??= [];
        mapControl.preloadobjs ??= [];
        mapControl.eventPointers ??= [];
        mapControl.entities ??= [];
        mapControl.music ??= [];
        mapControl.dialogues ??= [];
        mapControl.insides ??= [];
        mapControl.entityonly ??= [];
        mapControl.canfollowID ??= [];
        mapControl.discoveryids ??= [];
        mapControl.roundways ??= [];
        mapControl.autoevent ??= [];
        mapControl.mapflags ??= [];
        mapControl.tempfollowers ??= new();
        mapControl.entitysprite ??= new();
        mapControl.musicflags ??= [];
        mapControl.commandlines ??= [];
    }
}