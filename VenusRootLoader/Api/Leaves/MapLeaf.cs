using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverQueried.Global

namespace VenusRootLoader.Api.Leaves;

// TODO: Figure out the Unity prefab tooling
[ExposeFromVenus(withRegisterMethod: false)]
public sealed class MapLeaf : Leaf
{
    internal MapLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<AreaLeaf> Area { get; set; }

    public Vector3? DefaultCameraPositionOffsetFromTargetOverride
    {
        get => field?.magnitude <= 0.2 ? null : field;
        set => field = value is null || value.Value.magnitude <= 0.2f ? null : value;
    }

    public Vector3? DefaultCameraAnglesOffsetFromTargetOverride
    {
        get => field?.magnitude <= 0.2 ? null : field;
        set => field = value is null || value.Value.magnitude <= 0.2f ? null : value;
    }
    public Vector3 DefaultCameraLowerBounds { get; set; } = new(-999.0f, -999.0f, -999.0f);
    public Vector3 DefaultCameraUpperBounds { get; set; } = new(999.0f, 999.0f, 999.0f);
    public MapCameraMoveAroundCircleConfiguration? CameraMoveAroundCircleConfiguration { get; set; }

    public float InitialFogEndDistance { get; set; } = 300f;
    public Color InitialFogColor { get; set; } = Color.white;
    public bool HasSunRaysTopRightScreenEffect { get; set; }

    // TODO: Consider adding SkyBoxLeaf
    public Material? SkyboxMaterial { get; set; }
    public Color InitialAmbientLightColor { get; set; } = Color.gray;
    public float WindIntensity { get; set; } = 0.2f;
    public bool ForceAllFadersToFadeInsteadOfCulling { get; set; }
    public Color AllFadersFadingTint { get; set; } = Color.white;

    // TODO: Add BattleMapLeaf
    public MainManager.BattleMaps DefaultBattleMap { get; set; }

    // TODO: Consider adding BattleTransitionLeaf
    public MapControl.BattleLeafType BattleTransition { get; set; }

    public float ExpMultiplier
    {
        get;
        set
        {
            Guard.IsGreaterThanOrEqualTo(value, 0.0f, nameof(ExpMultiplier));
            field = value;
        }
    } = 1.0f;

    public Color DefaultBattleTransitionLeavesColor { get; set; } = Color.green;
    public bool DisableMusicChangeWhenEnteringBattle { get; set; }

    private ReadOnlyListWithCreate<MapMusic> InternalMusicsAvailable { get; } = new();
    public IReadOnlyList<MapMusic> MusicsAvailable => InternalMusicsAvailable;
    public bool KeepsExistingMusicPlayingOnLoad { get; set; }
    public List<MapMusicSelectionCondition> MusicSelectionConditions { get; } = new();

    public List<MapInside> Insides { get; } = new();
    public bool ForceRestoreCameraWhenExitingAnyInsideTransitionZone { get; set; }
    public bool DisablesInsideWhenCurrentInsideIsDifferent { get; set; }
    public bool SetCameraTargetToCurrentInsideWhileInside { get; set; }
    public float FadingSpeedWhenEnteringOrExitingAnInside { get; set; } = 0.2f;

    public Branch<DialogueLeaf> SpyDialogue { get; set; }

    public List<AnimIdLeaf> FollowerAnimIdsAllowed { get; } = new();

    public float MaximumYFollowerDistanceBeforeTeleport
    {
        get;
        set
        {
            Guard.IsGreaterThanOrEqualTo(value, 0.0f, nameof(ExpMultiplier));
            field = value;
        }
    } = 20.0f;

    // TODO: Consider patching the game to address the mess of the closemove field so it can be exposed

    // TODO: Patch out the Hazard logic and instead, have the collector set this to -150f
    public float AllEntitiesYPositionLowerBoundLimitBeforeRespawn { get; set; } = -50f;
    public bool IsFrozenMap { get; set; }

    // TODO: Recheck these 2 to name them better
    public bool MapEntitiesHaveRestrictedActiveRange { get; set; }
    public bool MapEntitiesAreKeptActive { get; set; }

    public string? MainMapTransformOverridePrefabPath { get; set; }
    public List<DiscoveryLeaf> DetectableDiscoveriesByDetectorMedal { get; } = new();
    public Branch<MapLeaf>? MapWhoProvidesEntitiesAndDialogues { get; set; }
    public bool DisallowAntCompassUsage { get; set; }
    public List<MapAutoEvent> AutomaticallyTriggeredEventsAfterLoad { get; } = new();
    public List<string> EventsGameObjectPrefabPaths { get; } = new();

    public Func<MapLeaf, GameObject>? PrefabInstantiator { get; set; }

    internal ILeavesRegistry<MapEntityLeaf> EntitiesRegistry { get; set; } = null!;
    internal ILeavesRegistry<MapDialogueLeaf> DialoguesRegistry { get; set; } = null!;

    public MapMusic AddMusicToMap(Branch<MusicLeaf>? music) =>
        InternalMusicsAvailable.CreateNew(id => new MapMusic(id)
        {
            Music = music,
            Map = this
        });
}

public sealed class MapCameraMoveAroundCircleConfiguration
{
    public Vector3 InitialCircleCenter { get; set; }
    public bool CameraFollowsTargetInYAxis { get; set; }

    public float? CameraMaxRadiusFromCenterPointAllowed
    {
        get => field is null or <= 0 ? null : field;
        set => field = value is null or <= 0 ? null : value;
    }
}

public sealed class MapMusic
{
    public int MusicIdInMap { get; }
    public required Branch<MapLeaf> Map { get; init; }
    public required Branch<MusicLeaf>? Music { get; set; }
    internal MapMusic(int musicIdInMap) => MusicIdInMap = musicIdInMap;
}

public sealed class MapMusicSelectionCondition
{
    public required Branch<FlagLeaf>? RequiredFlag { get; set; }

    public required MapMusic MapMusic
    {
        get;
        set
        {
            if (field is not null)
                Guard.IsEqualTo(value.Map, field.Map);
            field = value;
        }
    }
}

public sealed class MapInside
{
    public required string GameObjectPathInPrefab { get; set; }

    // TODO: Consider adding InsideTransitionLeaf
    public MapControl.InsideType TransitionWhenEnteringOrExiting { get; set; }
}

public sealed class MapAutoEvent
{
    public required Branch<FlagLeaf> AlreadyTriggeredFlag { get; set; }
    public required Branch<EventLeaf> EventToTriggerWhenFlagIsFalse { get; set; }
}