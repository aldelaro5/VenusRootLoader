using System;
using System.Collections.Generic;
using Editor.Attributes;
using UnityEngine;

// TODO: Add tooltips

[CreateAssetMenu(fileName = "NewMap", menuName = "MapLeaf", order = 0)]
public sealed class MapLeafScriptableObject : ScriptableObject
{
    [NonSerialized]
    public GameObject InternalPrefab = null;

    [PrefabAssetPath(nameof(InternalPrefab))]
    public string Prefab;

    [Branch(typeof(Areas))]
    public Branch Area;

    [SerializeField]
    [HideInInspector]
    private bool DefaultCameraPositionOffsetFromTargetOverrideHasValue;

    [Header("Camera settings")]
    [OptionalField(nameof(DefaultCameraPositionOffsetFromTargetOverrideHasValue))]
    public Vector3 DefaultCameraPositionOffsetFromTargetOverride = new Vector3(0f, 2.25f, -8.25f);

    [SerializeField]
    [HideInInspector]
    private bool DefaultCameraAnglesOffsetFromTargetOverrideHasValue;

    [OptionalField(nameof(DefaultCameraAnglesOffsetFromTargetOverrideHasValue))]
    public Vector3 DefaultCameraAnglesOffsetFromTargetOverride = new Vector3(10f, 0f, 0f);

    [Space]
    public Vector3 DefaultCameraLowerBounds = new Vector3(-999.0f, -999.0f, -999.0f);

    public Vector3 DefaultCameraUpperBounds = new Vector3(999.0f, 999.0f, 999.0f);

    [Space]
    [SerializeField]
    [HideInInspector]
    private bool CameraMovesAroundCircleHasValue;

    [OptionalClassField(nameof(CameraMovesAroundCircleHasValue))]
    public MapCameraMoveAroundCircleConfiguration CameraMovesAroundCircle;

    [Header("Graphics settings")]
    public float InitialFogEndDistance = 300f;

    public Color InitialFogColor = Color.white;

    [Space]
    public bool HasSunRaysTopRightScreenEffect;

    public Material SkyboxMaterial;
    public Color InitialAmbientLightColor = Color.gray;
    public float WindIntensity = 0.2f;

    [Space]
    public bool ForceAllFadersToFadeInsteadOfCulling;

    public Color AllFadersFadingTint = Color.white;

    [Header("Battle settings")]
    public BattleMaps DefaultBattleMap;

    public BattleLeafType BattleTransition;
    public Color DefaultBattleTransitionLeavesColor = Color.green;

    [Space]
    [Editor.Attributes.Min(0)]
    public float ExpMultiplier = 1.0f;

    public bool DisableMusicChangeWhenEnteringBattle;

    [Header("Music settings")]
    [Branch(typeof(Musics))]
    public List<Branch> MusicsAvailable;

    public bool KeepsExistingMusicPlayingOnLoad;
    public List<MapMusicSelectionCondition> MusicSelectionConditions;

    [Header("Insides settings")]
    public List<MapInside> Insides;

    public bool ForceRestoreCameraWhenExitingAnyInsideTransitionZone;
    public bool DisablesInsideWhenCurrentInsideIsDifferent;
    public bool SetCameraTargetToCurrentInsideWhileInside;
    public float FadingSpeedWhenEnteringOrExitingAnInside = 0.2f;

    [Header("Entities settings")]
    public float AllEntitiesYPositionLowerBoundLimitBeforeRespawn = -50f;

    public bool MapEntitiesHaveRestrictedActiveRange;
    public bool MapEntitiesAndEmoticonsAreActiveWhenOutOfRange;

    [Header("Followers settings")]
    [Branch(typeof(AnimIDs))]
    public List<Branch> FollowerAnimIdsAllowed;

    [Editor.Attributes.Min(0)]
    public float MaximumYFollowerDistanceBeforeTeleport = 20.0f;

    [Header("Miscellaneous settings")]
    [Branch(-203)]
    public Branch SpyDialogue;

    [NonSerialized]
    public Transform InternalMainMapTransformOverride = null;

    [TransformPathInPrefab(
        nameof(InternalMainMapTransformOverride),
        nameof(InternalPrefab),
        nameof(Prefab))]
    public string MainMapTransformOverride;

    public bool IsFrozenMap;

    [Branch(0, 49)]
    public List<Branch> DetectableDiscoveriesByDetectorMedal;

    [SerializeField]
    [HideInInspector]
    private bool MapWhoProvidesEntitiesAndDialoguesHasValue;

    [Branch(typeof(Maps), nameof(MapWhoProvidesEntitiesAndDialoguesHasValue))]
    public Branch RedirectsEntitiesAndDialoguesToAnotherMap;

    public bool DisallowAntCompassUsage;
    public List<MapAutoEvent> AutomaticallyTriggeredEventsAfterLoad;
    public List<MapEventTransform> EventsTransform;
}

[Serializable]
public sealed class MapCameraMoveAroundCircleConfiguration
{
    public Vector3 InitialCircleCenter;
    public bool CameraFollowsTargetInYAxis;

    [SerializeField]
    [HideInInspector]
    private bool CameraMaxRadiusFromCenterPointAllowedHasValue = true;

    [OptionalField(nameof(CameraMaxRadiusFromCenterPointAllowedHasValue))]
    [Editor.Attributes.Min(float.Epsilon)]
    public float CameraMaxRadiusFromCenterPointAllowed = float.Epsilon;
}

[Serializable]
public sealed class MapMusicSelectionCondition
{
    [SerializeField]
    [HideInInspector]
    private bool RequiredFlagHasValue = true;

    [Branch(0, 749, nameof(RequiredFlagHasValue))]
    public Branch RequiredFlag;

    [Editor.Attributes.Min(0)]
    public int MusicIndexInMap;
}

[Serializable]
public sealed class MapInside
{
    [NonSerialized]
    public Transform InternalTransform;

    [TransformPathInPrefab(
        nameof(InternalTransform),
        nameof(MapLeafScriptableObject.InternalPrefab),
        nameof(MapLeafScriptableObject.Prefab))]
    public string Transform;

    public InsideType TransitionWhenEnteringOrExiting;
}

[Serializable]
public sealed class MapAutoEvent
{
    [Branch(0, 749)]
    public Branch AlreadyTriggeredFlag;

    [Branch(0, 228)]
    public Branch EventToTriggerWhenFlagIsFalse;
}

[Serializable]
public sealed class Branch
{
    [SerializeField]
    [HideInInspector]
    public BranchCreatorKind CreatorKind = BranchCreatorKind.BaseGame;

    public string CustomCreatorId;
    public string NamedId;
}

[Serializable]
public sealed class MapEventTransform
{
    [NonSerialized]
    public Transform InternalTransform;

    [TransformPathInPrefab(
        nameof(InternalTransform),
        nameof(MapLeafScriptableObject.InternalPrefab),
        nameof(MapLeafScriptableObject.Prefab))]
    public string Transform;
}