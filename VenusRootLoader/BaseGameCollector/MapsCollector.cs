using AssetsTools.NET;
using AssetsTools.NET.Extra;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MapsCollector : IBaseGameCollector
{
    private static readonly string[] MapNamedIds = Enum.GetNames(typeof(MainManager.Maps)).ToArray();

    private static readonly Dictionary<int, (string[] Names, string[] Data)> MapsEntityData =
        Enumerable.Range(0, MapNamedIds.Length)
            .ToDictionary(
                x => x,
                x => (RootCollector.ReadTextAssetLines($"{TextAssetPaths.DataMapEntitiesDirectory}/Names/{x}Names"),
                    RootCollector.ReadTextAssetLines($"{TextAssetPaths.DataMapEntitiesDirectory}/{x}")));

    private static readonly string[] TestRoomTextData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataTestRoomMapDialoguesPath);
    
    private static readonly Dictionary<string, Dictionary<int, string[]>> MapsDialogues = new();

    private readonly AssetsManager _assetsManager = new();
    private readonly AssetsFileInstance _resourcesFileInstance;
    private readonly Dictionary<int, AssetTypeValueField> _mapControlBaseFieldsByGameIds;
    private readonly string _assemblyCSharpFileName;
    private readonly string _gameBundlePath;
    private readonly string _gameManagedDirectoryPath;
    private readonly string _classDataTpkPath;

    private readonly GameExecutionContext _gameExecutionContext;
    private readonly BudLoaderContext _budLoaderContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<MapsCollector> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;
    private readonly ILeavesRegistry<MusicLeaf> _musicsRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly ILeavesRegistry<CommonDialogueLeaf> _commonDialoguesRegistry;
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsRegistry;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly ILeavesRegistry<EventLeaf> _eventsRegistry;
    private readonly IMapEntityTextAssetParser _mapEntityTextAssetParser;
    private readonly IRegistryResolver _registryResolver;

    public MapsCollector(
        GameExecutionContext gameExecutionContext,
        BudLoaderContext budLoaderContext,
        IFileSystem fileSystem,
        ILogger<MapsCollector> logger,
        ILoggerFactory loggerFactory,
        ILeavesRegistry<MapLeaf> mapsRegistry,
        ILeavesRegistry<AreaLeaf> areasRegistry,
        ILeavesRegistry<MusicLeaf> musicsRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry,
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry,
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        ILeavesRegistry<EventLeaf> eventsRegistry,
        IMapEntityTextAssetParser mapEntityTextAssetParser,
        IRegistryResolver registryResolver)
    {
        _budLoaderContext = budLoaderContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _mapsRegistry = mapsRegistry;
        _areasRegistry = areasRegistry;
        _musicsRegistry = musicsRegistry;
        _flagsRegistry = flagsRegistry;
        _commonDialoguesRegistry = commonDialoguesRegistry;
        _animIdsRegistry = animIdsRegistry;
        _discoveriesRegistry = discoveriesRegistry;
        _eventsRegistry = eventsRegistry;
        _mapEntityTextAssetParser = mapEntityTextAssetParser;
        _registryResolver = registryResolver;
        _gameExecutionContext = gameExecutionContext;

        foreach (string mapName in MapNamedIds)
        {
            MapsDialogues[mapName] = new();
            for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
            {
                string[] itemLanguageData = Resources
                    .Load<TextAsset>(
                        $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataDialoguesLocalizedMapsDirectory}/{mapName}")
                    .text
                    .Split(StringUtils.NewlineSplitDelimiter);
                MapsDialogues[mapName].Add(i, itemLanguageData);
            }
        }

        _assemblyCSharpFileName = _fileSystem.Path.GetFileName(typeof(MapControl).Assembly.Location);
        _gameBundlePath = _fileSystem.Path.Combine(_gameExecutionContext.DataDir, "data.unity3d");
        _gameManagedDirectoryPath = _fileSystem.Path.Combine(_gameExecutionContext.DataDir, "Managed");
        _classDataTpkPath = _fileSystem.Path.Combine(_budLoaderContext.LoaderPath, "classdata.tpk");

        _assetsManager.UseQuickLookup = true;
        _assetsManager.UseTemplateFieldCache = true;
        _assetsManager.UseMonoTemplateFieldCache = true;
        _assetsManager.UseRefTypeManagerCache = true;
        _assetsManager.LoadClassPackage(_classDataTpkPath);
        BundleFileInstance bundleInstance = _assetsManager.LoadBundleFile(_gameBundlePath);
        _resourcesFileInstance = _assetsManager.LoadAssetsFileFromBundle(bundleInstance, "resources.assets");

        AssetsFileInstance globalManagersFileInstance =
            _assetsManager.LoadAssetsFileFromBundle(bundleInstance, "globalgamemanagers");
        AssetsFile globalManagersFile = globalManagersFileInstance.file;
        _assetsManager.LoadClassDatabaseFromPackage(globalManagersFile.Metadata.UnityVersion);
        _assetsManager.MonoTempGenerator = new MonoCecilTempGenerator(_gameManagedDirectoryPath);

        _mapControlBaseFieldsByGameIds = CollectMapControlBaseFields();
    }

    private Dictionary<int, AssetTypeValueField> CollectMapControlBaseFields()
    {
        Dictionary<int, AssetTypeValueField> mapControlBaseFieldsByGameId = new();
        AssetsFile resourcesFile = _resourcesFileInstance.file;
        Dictionary<int, AssetTypeReference> scriptInfos =
            AssetHelper.GetAssetsFileScriptInfos(_assetsManager, _resourcesFileInstance);
        int mapControlScriptIndex = scriptInfos
            .Single(x => x.Value.AsmName == _assemblyCSharpFileName
                         && x.Value.Namespace == ""
                         && x.Value.ClassName == nameof(MapControl))
            .Key;

        foreach (AssetFileInfo assetFileInfo in resourcesFile.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            if (assetFileInfo.GetScriptIndex(resourcesFile) != mapControlScriptIndex)
                continue;

            AssetTypeValueField mapControlBaseField =
                _assetsManager.GetBaseField(_resourcesFileInstance, assetFileInfo);
            int mapGameId = mapControlBaseField[nameof(MapControl.mapid)].AsInt;

            if (mapControlBaseFieldsByGameId.ContainsKey(mapGameId))
            {
                if (!TryGetBaseFieldFromReference(
                        mapControlBaseField["m_GameObject"],
                        _resourcesFileInstance,
                        _assetsManager,
                        out AssetTypeValueField? gameObjectBaseField))
                {
                    continue;
                }

                string gameObjectName = gameObjectBaseField["m_Name"].AsString;
                if (gameObjectName != ((MainManager.Maps)mapGameId).ToString())
                    continue;
                mapControlBaseFieldsByGameId.Remove(mapGameId);
            }

            mapControlBaseFieldsByGameId.Add(mapGameId, mapControlBaseField);
        }

        return mapControlBaseFieldsByGameId;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < MapNamedIds.Length; i++)
        {
            (string[] Names, string[] Data) mapEntityData = MapsEntityData[i];
            MapLeaf mapLeaf = _mapsRegistry.RegisterExisting(i, MapNamedIds[i], baseGameId);
            mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
                _loggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
                IdSequenceDirection.Increment);
            mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
                _loggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
                IdSequenceDirection.Increment);

            for (int j = 0; j < mapEntityData.Data.Length; j++)
            {
                string mapEntityText = mapEntityData.Data[j];
                string mapEntityName = mapEntityData.Names[j];
                _mapEntityTextAssetParser.FromTextAssetSerializedString(
                    mapLeaf,
                    baseGameId,
                    mapLeaf.EntitiesRegistry.LeavesByGameIds.Count,
                    mapEntityName,
                    mapEntityText);
            }

            if (i == 0)
            {
                for (int j = 0; j < TestRoomTextData.Length; j++)
                {
                    MapDialogueLeaf mapDialogueLeaf =
                        mapLeaf.DialoguesRegistry.RegisterExisting(j, j.ToString(), baseGameId);
                    mapDialogueLeaf.Map = mapLeaf;
                    mapDialogueLeaf.LocalizedText[0] = TestRoomTextData[j];
                }

                continue;
            }

            for (int j = 0; j < MapsDialogues[mapLeaf.NamedId].Values.Max(x => x.Length); j++)
            {
                MapDialogueLeaf mapDialogueLeaf =
                    mapLeaf.DialoguesRegistry.RegisterExisting(j, j.ToString(), baseGameId);
                mapDialogueLeaf.Map = mapLeaf;
            }

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                for (int k = 0; k < MapsDialogues[mapLeaf.NamedId][j].Length; k++)
                {
                    MapDialogueLeaf mapDialogueLeaf = mapLeaf.DialoguesRegistry.LeavesByGameIds[k];
                    mapDialogueLeaf.LocalizedText[j] = MapsDialogues[mapLeaf.NamedId][j][k];
                }
            }
        }

        // This last step is needed because while we have filled all the backing fields of the entity, the derived class
        // might need to synchronize itself with the data we just filled. This only needs to be done once per map entity
        // because we just filled them from external data, but any further modification should get synchronized immediately.
        // It also needs to be done after every map have been added so references across them works as expected.
        foreach (MapLeaf mapLeaf in _mapsRegistry.LeavesByGameIds.Values)
        {
            foreach (MapEntityLeaf mapEntity in mapLeaf.EntitiesRegistry.LeavesByGameIds.Values)
            {
                // Fixes a base game issue where this entity has invalid, but normally inaccessible dialogues data
                if (mapLeaf.NamedId == nameof(MainManager.Maps.BugariaEndThrone) && mapEntity.GameId == 16)
                {
                    mapEntity.InternalDialogues.Clear();
                    mapEntity.InternalDialogues.Add(new(new(-1, 0, 0)));
                }
                mapEntity.InitializeFromExisting(_registryResolver);
            }

            // Needs to be done here since we need all MapLeaf to be registered
            FillMapControlDataIntoMapLeaf(
                _mapControlBaseFieldsByGameIds[mapLeaf.GameId],
                _assetsManager,
                _resourcesFileInstance,
                mapLeaf);
        }

        _assetsManager.UnloadAll(true);
        _logger.LogInformation(
            "Collected and registered {MapsAmount} base game maps",
            MapNamedIds.Length);
    }


    private static bool TryGetBaseFieldFromReference(
        AssetTypeValueField referenceField,
        AssetsFileInstance ownFileInstance,
        AssetsManager manager,
        [NotNullWhen(true)] out AssetTypeValueField? baseField)
    {
        int fileId = referenceField["m_FileID"].AsInt;
        long pathId = referenceField["m_PathID"].AsLong;
        if (pathId == 0 && fileId == 0)
        {
            baseField = null;
            return false;
        }

        AssetsFileInstance fileInstance = ownFileInstance;
        if (fileId != 0)
            fileInstance = manager.LoadAssetsFileFromBundle(ownFileInstance.parentBundle, fileId);
        baseField = manager.GetBaseField(fileInstance, pathId);
        return true;
    }

    private void FillMapControlDataIntoMapLeaf(
        AssetTypeValueField mapControlBaseField,
        AssetsManager manager,
        AssetsFileInstance resourcesFileInstance,
        MapLeaf mapLeaf)
    {
        int areaGameId = mapControlBaseField[nameof(MapControl.areaid)].AsInt;

        mapLeaf.Area = _areasRegistry.LeavesByGameIds[areaGameId];

        mapLeaf.DefaultCameraPositionOffsetFromTargetOverride =
            ExtractVector3FromAssetValueField(mapControlBaseField[nameof(MapControl.camoffset)]);
        mapLeaf.DefaultCameraPositionOffsetFromTargetOverride =
            ExtractVector3FromAssetValueField(mapControlBaseField[nameof(MapControl.camangle)]);
        mapLeaf.DefaultCameraLowerBounds =
            ExtractVector3FromAssetValueField(mapControlBaseField[nameof(MapControl.camlimitneg)]);
        mapLeaf.DefaultCameraUpperBounds =
            ExtractVector3FromAssetValueField(mapControlBaseField[nameof(MapControl.camlimitpos)]);
        if (mapControlBaseField[nameof(MapControl.rotatecam)].AsBool)
        {
            mapLeaf.CameraMoveAlongCircleConfiguration = new()
            {
                InitialCircleCenter =
                    ExtractVector3FromAssetValueField(mapControlBaseField[nameof(MapControl.centralpoint)]),
                CameraFollowsTargetInYAxis = mapControlBaseField[nameof(MapControl.tieYtoplayer)].AsBool,
                CameraMaxRadiusFromCenterPointAllowed = mapControlBaseField[nameof(MapControl.tetherdistance)].AsFloat
            };
        }

        mapLeaf.InitialFogEndDistance = mapControlBaseField[nameof(MapControl.fogend)].AsFloat;
        mapLeaf.InitialFogColor = ExtractColorFromAssetValueField(mapControlBaseField[nameof(MapControl.fogcolor)]);
        mapLeaf.HasSunRaysTopRightScreenEffect = mapControlBaseField[nameof(MapControl.screeneffect)].AsInt ==
                                                 (int)MapControl.ScreenEffects.SunRaysTopRight;
        AssetTypeValueField skyboxMatRef = mapControlBaseField[nameof(MapControl.skyboxmat)];
        if (TryGetBaseFieldFromReference(
                skyboxMatRef,
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? skyboxMaterialBaseField))
        {
            string skyboxMaterialName = skyboxMaterialBaseField["m_Name"].AsString;
            mapLeaf.SkyboxMaterial = Resources.Load<Material>($"Materials/Skybox/{skyboxMaterialName}");
        }

        mapLeaf.InitialAmbientLightColor =
            ExtractColorFromAssetValueField(mapControlBaseField[nameof(MapControl.globallight)]);
        mapLeaf.WindIntensity = mapControlBaseField[nameof(MapControl.windintensity)].AsFloat;
        mapLeaf.ForceAllFadersToFadeInsteadOfCulling = mapControlBaseField[nameof(MapControl.faderchange)].AsBool;
        mapLeaf.AllFadersFadingTint = ExtractColorFromAssetValueField(mapControlBaseField[nameof(MapControl.skycolor)]);

        mapLeaf.DefaultBattleMap = (MainManager.BattleMaps)mapControlBaseField[nameof(MapControl.battlemap)].AsInt;
        mapLeaf.BattleTransition =
            (MapControl.BattleLeafType)mapControlBaseField[nameof(MapControl.battleleaftype)].AsInt;
        mapLeaf.ExpMultiplier = mapControlBaseField[nameof(MapControl.expmulti)].AsFloat;
        mapLeaf.DefaultBattleTransitionLeavesColor =
            ExtractColorFromAssetValueField(mapControlBaseField[nameof(MapControl.battleleafcolor)]);
        mapLeaf.DisableMusicChangeWhenEnteringBattle = mapControlBaseField[nameof(MapControl.nobattlemusic)].AsBool;

        List<MapMusic> orderedMapMusics = new();
        AssetTypeValueField musicAudioClipsArray = mapControlBaseField[nameof(MapControl.music)][nameof(Array)];
        for (int i = 0; i < musicAudioClipsArray.AsArray.size; i++)
        {
            AssetTypeValueField audioClipValueField = musicAudioClipsArray[i];
            if (!TryGetBaseFieldFromReference(
                    audioClipValueField,
                    resourcesFileInstance,
                    manager,
                    out AssetTypeValueField? audioClipBaseField))
            {
                MapMusic silenceMapMusic = mapLeaf.AddMusicToMap(null);
                orderedMapMusics.Add(silenceMapMusic);
                continue;
            }

            string audioClipName = audioClipBaseField["m_Name"].AsString;
            MusicLeaf musicLeaf = _musicsRegistry.LeavesByEffectiveIds[audioClipName];
            MapMusic mapMusic = mapLeaf.AddMusicToMap(musicLeaf);
            orderedMapMusics.Add(mapMusic);
        }

        if (mapLeaf.MusicsAvailable.Count == 0)
        {
            MapMusic silenceMapMusic = mapLeaf.AddMusicToMap(null);
            orderedMapMusics.Add(silenceMapMusic);
        }

        mapLeaf.KeepsExistingMusicPlayingOnLoad = mapControlBaseField[nameof(MapControl.keepmusic)].AsBool;
        AssetTypeValueField musicFlagsArray = mapControlBaseField[nameof(MapControl.musicflags)][nameof(Array)];
        foreach (AssetTypeValueField musicFlagValueField in musicFlagsArray)
        {
            Vector2Int musicFlagValue = ExtractVector2IntFromAssetValueField(musicFlagValueField);
            FlagLeaf? flagLeaf = musicFlagValue.x >= 0
                ? _flagsRegistry.LeavesByGameIds[musicFlagValue.x]
                : null;
            MapMusic mapMusic = orderedMapMusics[musicFlagValue.y];
            mapLeaf.MusicSelectionConditions.Add(
                new()
                {
                    RequiredFlag = flagLeaf is null ? null : new(flagLeaf),
                    MapMusic = mapMusic
                });
        }

        AssetTypeValueField insidesArray = mapControlBaseField[nameof(MapControl.insides)][nameof(Array)];
        AssetTypeValueField insideTypesArray = mapControlBaseField[nameof(MapControl.insidetypes)][nameof(Array)];
        for (int i = 0; i < insidesArray.AsArray.size; i++)
        {
            AssetTypeValueField insideValueField = insidesArray[i];
            if (!TryGetBaseFieldFromReference(
                    insideValueField,
                    resourcesFileInstance,
                    manager,
                    out AssetTypeValueField? insideGameObjectBaseField))
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"Can't find the GameObject of an inside, map: {mapLeaf.NamedId}, index: {i}");
            }

            if (!TryGetBaseFieldFromReference(
                    insideGameObjectBaseField["m_Component"][nameof(Array)][0]["component"],
                    resourcesFileInstance,
                    manager,
                    out AssetTypeValueField? insideTransformBaseField))
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"Can't find the Transform of an inside's GameObject, map: {mapLeaf.NamedId}, index: {i}");
            }

            string transformPath = GetTransformPathFromRoot(
                insideTransformBaseField,
                resourcesFileInstance,
                manager,
                new());
            MapControl.InsideType insideType = i < insideTypesArray.AsArray.size
                ? (MapControl.InsideType)insideTypesArray[i].AsInt
                : MapControl.InsideType.Stretch;

            mapLeaf.Insides.Add(
                new()
                {
                    GameObjectPathInPrefab = transformPath,
                    TransitionWhenEnteringOrExiting = insideType
                });
        }

        mapLeaf.ForceRestoreCameraWhenExitingAnyInsideTransitionZone =
            mapControlBaseField[nameof(MapControl.tieinsidedoorentities)].AsBool;
        mapLeaf.DisablesInsideWhenCurrentInsideIsDifferent = mapControlBaseField[nameof(MapControl.hideinsides)].AsBool;
        mapLeaf.SetCameraTargetToCurrentInsideWhileInside =
            mapControlBaseField[nameof(MapControl.setinsidecenter)].AsBool;
        mapLeaf.FadingSpeedWhenEnteringOrExitingAnInside = mapControlBaseField[nameof(MapControl.fadingspeed)].AsFloat;

        int dialogueGameId = mapControlBaseField[nameof(MapControl.tattleid)].AsInt;
        DialogueLeaf spyDialogue = dialogueGameId < 0
            ? _commonDialoguesRegistry.LeavesByGameIds[dialogueGameId]
            : mapLeaf.DialoguesRegistry.LeavesByGameIds[dialogueGameId];
        mapLeaf.SpyDialogue = spyDialogue;

        AssetTypeValueField canFollowIdsArray = mapControlBaseField[nameof(MapControl.canfollowID)][nameof(Array)];
        foreach (AssetTypeValueField followerValueField in canFollowIdsArray)
        {
            AnimIdLeaf animIdLeaf = _animIdsRegistry.LeavesByGameIds[followerValueField.AsInt];
            mapLeaf.FollowerAnimIdsAllowed.Add(animIdLeaf);
        }

        mapLeaf.MaximumYFollowerDistanceBeforeTeleport = mapControlBaseField[nameof(MapControl.followerylimit)].AsFloat;

        mapLeaf.AllEntitiesYPositionLowerBoundLimitBeforeRespawn =
            mapControlBaseField[nameof(MapControl.ylimit)].AsFloat;
        mapLeaf.IsFrozenMap = mapControlBaseField[nameof(MapControl.icemap)].AsBool;
        mapLeaf.MapEntitiesHaveRestrictedActiveRange = mapControlBaseField[nameof(MapControl.limitbehavior)].AsBool;
        mapLeaf.MapEntitiesAreKeptActive = mapControlBaseField[nameof(MapControl.keepobjectsactive)].AsBool;

        if (TryGetBaseFieldFromReference(
                mapControlBaseField[nameof(MapControl.mainmesh)],
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? mainMeshBaseField))
        {
            mapLeaf.MainMapTransformOverridePrefabPath = GetMapMainMeshTransformPathFromRoot(
                mainMeshBaseField,
                resourcesFileInstance,
                manager,
                mapLeaf,
                new());
        }

        AssetTypeValueField discoveryIdsArray = mapControlBaseField[nameof(MapControl.discoveryids)][nameof(Array)];
        foreach (AssetTypeValueField discoveryValueField in discoveryIdsArray)
        {
            DiscoveryLeaf discoveryLeaf = _discoveriesRegistry.LeavesByGameIds[discoveryValueField.AsInt];
            mapLeaf.DetectableDiscoveriesByDetectorMedal.Add(discoveryLeaf);
        }

        int readFromOtherMapGameId = mapControlBaseField[nameof(MapControl.readdatafromothermap)].AsInt;
        if (readFromOtherMapGameId > 0)
            mapLeaf.MapWhoProvidesEntitiesAndDialogues = _mapsRegistry.LeavesByGameIds[readFromOtherMapGameId];
        mapLeaf.TimeInFramesOnLoadBeforeUpdatingFadersAndLoadingZonesEnablement =
            mapControlBaseField[nameof(MapControl.alivetime)].AsFloat;
        mapLeaf.DisallowAntCompassUsage = mapControlBaseField[nameof(MapControl.cantcompass)].AsBool;

        AssetTypeValueField autoEventsArray = mapControlBaseField[nameof(MapControl.autoevent)][nameof(Array)];
        foreach (AssetTypeValueField autoEventValueField in autoEventsArray)
        {
            Vector2 autoEvent = ExtractVector2FromAssetValueField(autoEventValueField);
            FlagLeaf flagLeaf = _flagsRegistry.LeavesByGameIds[(int)autoEvent.x];
            EventLeaf eventLeaf = _eventsRegistry.LeavesByGameIds[(int)autoEvent.y];
            mapLeaf.AutomaticallyTriggeredEventsAfterLoad.Add(
                new()
                {
                    AlreadyTriggeredFlag = flagLeaf,
                    EventToTriggerWhenFlagIsFalse = eventLeaf
                });
        }

        AssetTypeValueField eventPointersArray = mapControlBaseField[nameof(MapControl.eventPointers)][nameof(Array)];
        for (int i = 0; i < eventPointersArray.AsArray.size; i++)
        {
            AssetTypeValueField eventPointerValueField = eventPointersArray[i];
            if (!TryGetBaseFieldFromReference(
                    eventPointerValueField,
                    resourcesFileInstance,
                    manager,
                    out AssetTypeValueField? eventPointerGameObjectBaseField))
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"Can't find the GameObject of an event pointer, map: {mapLeaf.NamedId}, index: {i}");
            }

            if (!TryGetBaseFieldFromReference(
                    eventPointerGameObjectBaseField["m_Component"][nameof(Array)][0]["component"],
                    resourcesFileInstance,
                    manager,
                    out AssetTypeValueField? eventPointerTransformBaseField))
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"Can't find the Transform of an event pointer's GameObject, map: {mapLeaf.NamedId}, index: {i}");
            }

            string transformPath = GetTransformPathFromRoot(
                eventPointerTransformBaseField,
                resourcesFileInstance,
                manager,
                new());
            mapLeaf.EventsGameObjectPrefabPaths.Add(transformPath);
        }
    }

    private static string GetTransformPathFromRoot(
        AssetTypeValueField transformBaseField,
        AssetsFileInstance resourcesFileInstance,
        AssetsManager manager,
        Stack<string> pathPartsStack)
    {
        if (!TryGetBaseFieldFromReference(
                transformBaseField["m_GameObject"],
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? gameObjectBaseField))
        {
            ThrowHelper.ThrowInvalidDataException("Can't find the GameObject of a Transform");
        }

        pathPartsStack.Push(gameObjectBaseField["m_Name"].AsString);

        if (TryGetBaseFieldFromReference(
                transformBaseField["m_Father"],
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? parentTransformBaseField))
        {
            return GetTransformPathFromRoot(parentTransformBaseField, resourcesFileInstance, manager, pathPartsStack);
        }

        StringBuilder sb = new();
        while (pathPartsStack.Count > 0)
        {
            sb.Append('/');
            sb.Append(pathPartsStack.Pop());
        }

        return sb.ToString();
    }

    private static string GetMapMainMeshTransformPathFromRoot(
        AssetTypeValueField transformBaseField,
        AssetsFileInstance resourcesFileInstance,
        AssetsManager manager,
        MapLeaf mapLeaf,
        Stack<string> pathPartsStack)
    {
        if (!TryGetBaseFieldFromReference(
                transformBaseField["m_GameObject"],
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? gameObjectBaseField))
        {
            ThrowHelper.ThrowInvalidDataException("Can't find the GameObject of a Transform");
        }

        string name = gameObjectBaseField["m_Name"].AsString;
        if (name == mapLeaf.NamedId)
            return $"/{name}";
        if (mapLeaf.GameId != (int)MainManager.Maps.BugariaCommercial)
            return $"/{mapLeaf.NamedId}/{name}";

        pathPartsStack.Push(name);

        if (TryGetBaseFieldFromReference(
                transformBaseField["m_Father"],
                resourcesFileInstance,
                manager,
                out AssetTypeValueField? parentTransformBaseField))
        {
            return GetTransformPathFromRoot(parentTransformBaseField, resourcesFileInstance, manager, pathPartsStack);
        }

        StringBuilder sb = new();
        while (pathPartsStack.Count > 0)
        {
            sb.Append('/');
            sb.Append(pathPartsStack.Pop());
        }

        return sb.ToString();
    }

    private static Vector2Int ExtractVector2IntFromAssetValueField(AssetTypeValueField assetValueField)
    {
        int x = assetValueField[nameof(Vector2.x)].AsInt;
        int y = assetValueField[nameof(Vector2.y)].AsInt;
        return new Vector2Int(x, y);
    }

    private static Vector2 ExtractVector2FromAssetValueField(AssetTypeValueField assetValueField)
    {
        float x = assetValueField[nameof(Vector2.x)].AsFloat;
        float y = assetValueField[nameof(Vector2.y)].AsFloat;
        return new Vector2(x, y);
    }

    private static Vector3 ExtractVector3FromAssetValueField(AssetTypeValueField assetValueField)
    {
        float x = assetValueField[nameof(Vector3.x)].AsFloat;
        float y = assetValueField[nameof(Vector3.y)].AsFloat;
        float z = assetValueField[nameof(Vector3.z)].AsFloat;
        return new Vector3(x, y, z);
    }

    private static Color ExtractColorFromAssetValueField(AssetTypeValueField assetValueField)
    {
        float r = assetValueField[nameof(Color.r)].AsFloat;
        float g = assetValueField[nameof(Color.g)].AsFloat;
        float b = assetValueField[nameof(Color.b)].AsFloat;
        float a = assetValueField[nameof(Color.a)].AsFloat;
        return new Color(r, g, b, a);
    }
}