using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher allows to load base game and custom maps by not just redirecting the prefab from the resources, but also
/// to take over the instantiation part of it. The latter is what makes this patch necessary instead of a simple resources
/// redirection because it prevents having the original prefab being kept in the not saved special scene for custom maps.
/// This happens for custom maps because some fields in <see cref="MapControl"/> needs to be set that references children
/// of the prefab and setting these fields causes Unity to hold on to the resources since something holds a reference to it.
/// This can lead to leaking maps memory so to prevent this, we instantiate the moment we have a prefab and only after deal with
/// the <see cref="MapControl"/>.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.LoadMap(int)"/>: Replace both the Resources.Load and Instantiate call to let us return our own map prefab.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MapsLoadingTopLevelPatcher : ITopLevelPatcher
{
    private static MapsLoadingTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    public MapsLoadingTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _mapsRegistry = mapsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(MapsLoadingTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.LoadMap), typeof(int))]
    private static IEnumerable<CodeInstruction> RedirectMapPrefabs(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo resourcesLoadMethod = typeof(UnityEngine.Resources)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(x => x.Name == nameof(UnityEngine.Resources.Load)
                         && x is { ContainsGenericParameters: false, ReturnParameter: not null }
                         && x.GetParameters().Length == 1);

        matcher.MatchStartForward(CodeMatch.Calls(resourcesLoadMethod));
        while (matcher.Instruction.opcode != OpCodes.Isinst)
            matcher.SetInstructionAndAdvance(Code.Nop);
        matcher.SetInstruction(Transpilers.EmitDelegate(PatchMapPrefab));

        return matcher.Instructions();
    }

    private static GameObject PatchMapPrefab(string resourcesPath)
    {
        int mapEffectiveIdStart = resourcesPath.LastIndexOf('/') + 1;
        string mapEffectiveId = resourcesPath[mapEffectiveIdStart..];
        MapLeaf map = _instance._mapsRegistry.LeavesByEffectiveIds[mapEffectiveId];
        GameObject mapPrefab = map.PrefabLoader.LoadAsset();
        // It is normally -1, but if it's not, it would mean that the loader already created the prefab in the Main scene
        // so we don't need to instantiate it.
        if (mapPrefab.gameObject.scene.buildIndex == -1)
            mapPrefab = Object.Instantiate(mapPrefab);

        MapControl mapControl = mapPrefab.GetComponent<MapControl>();
        // This will be the case for custom maps since we're creating a new one.
        if (mapControl == null)
            mapControl = mapPrefab.AddComponent<MapControl>();
        PatchMapControl(map, mapPrefab, mapControl);
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

        List<GameObject> insides = new();
        foreach (MapInside x in map.Insides)
            insides.Add(mapPrefab.transform.Find(x.GameObjectPathInPrefab).gameObject);
        mapControl.insides = insides.ToArray();
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
        mapControl.keepobjectsactive = map.MapEntitiesAndEmoticonsAreActiveWhenOutOfRange;

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

        List<GameObject> eventPointers = new();
        foreach (string x in map.EventsGameObjectPrefabPaths)
            eventPointers.Add(mapPrefab.transform.Find(x).gameObject);
        mapControl.eventPointers = eventPointers
            .ToArray();

        SimulateUnityCollectionDeserialisationLogic(mapControl);
    }

    // If a collection is serialized via the inspector, it's impossible it deserialized to null, and it will instead be an empty
    // collection. Since it's possible we're creating the MapControl, we need to simulate what Unity would have done for compatibility.
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