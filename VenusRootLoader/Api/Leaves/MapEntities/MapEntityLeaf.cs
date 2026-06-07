using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities;

public abstract class MapEntityLeaf : Leaf
{
    public string BaseGameObjectName { get; set; } = $"Unnamed {nameof(MapEntityLeaf)}";
    public Branch<MapLeaf> Map { get; internal set; }

    protected internal NPCControl.NPCType OriginalType { get; internal set; }
    internal abstract NPCControl.NPCType Type { get; }

    protected internal NPCControl.ObjectTypes OriginalObjectType { get; internal set; }
    internal abstract NPCControl.ObjectTypes ObjectType { get; }

    protected internal NPCControl.Interaction OriginalInteraction { get; internal set; }
    internal abstract NPCControl.Interaction Interaction { get; }

    internal MapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal abstract void InitializeFromNew();
    internal abstract void InitializeFromExisting(IRegistryResolver registryResolver);

    internal int[] OriginalRequires { get; } = new int[10];
    internal int[] OriginalLimits { get; } = new int[10];

    public List<Branch<FlagLeaf>> RequiredFlags { get; } = new();
    public List<LimitFlag> LimitedToFlags { get; } = new();

    public virtual Vector3 EntityStartingPosition
    {
        get => InternalStartingPosition;
        set => InternalStartingPosition = value;
    }

    public Vector3 TransformEulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }
    public Color TagColor { get => InternalTagColor; set => InternalTagColor = value; }

    public int? InsideId
    {
        get => InternalInsideId < 0 ? null : InternalInsideId;
        set => InternalInsideId = value ?? -1;
    }

    public int? RegionalFlagId
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    internal bool IsReturnToHeightOriginallyInt { get; set; }
    internal bool InternalReturnToHeight { get; set; } = true;
    internal int InternalInsideId { get; set; } = -1;
    internal Color InternalTagColor { get; set; }

    internal NPCControl.ActionBehaviors InternalOutOfRangeBehavior { get; set; }
    internal float InternalOutOfRangeActionFrequency { get; set; } = 200f;
    internal NPCControl.ActionBehaviors InternalInRangeBehavior { get; set; }
    internal float InternalInRangeActionFrequency { get; set; } = 200f;

    internal NPCControl.DeathType InternalDeathType { get; set; }
    internal int InternalAnimIdOrItemId { get; set; } = -1;
    internal bool InternalIsFlipped { get; set; }
    internal Vector3 InternalStartingPosition { get; set; }
    internal float InternalInitialHeight { get; set; }
    internal float InternalBobRange { get; set; }
    internal float InternalBobSpeed { get; set; }

    internal float InternalCcolHeight { get; set; } = 2f;
    internal float InternalCcolRadius { get; set; } = 0.5f;

    internal Vector3 InternalEulerAngles { get; set; }
    internal float InternalRadius { get; set; }
    internal float InternalTimer { get; set; } = -1f;

    internal float InternalSpeed { get; set; } = 5f;

    internal float InternalSpeedMultiplier { get; set; } = 1f;
    internal float InternalRadiusLimit { get; set; } = 6f;
    internal float InternalWanderRadius { get; set; } = 3f;
    internal float InternalTeleportRadius { get; set; } = 9f;

    internal float InternalFreezeTime { get; set; } = 600f;
    internal Vector3 InternalFreezeSize { get; set; } = Vector3.one;
    internal Vector3 InternalFreezeOffset { get; set; } = Vector3.zero;

    internal int InternalActivationFlagId { get; set; } = -1;
    internal int InternalRegionalFlagId { get; set; } = -1;

    internal int InternalSpyDialogueId { get; set; } = -1;
    internal int InternalEventId { get; set; } = -1;

    internal bool InternalHaxBoxCol { get; set; }
    internal bool InternalBoxColIsTrigger { get; set; }
    internal Vector3 InternalBoxColSize { get; set; } = Vector3.one;
    internal Vector3 InternalBoxColCenter { get; set; }

    internal int[] OriginalData { get; } = new int[10];
    internal List<Ref<int>> InternalData { get; } = new();

    internal Vector3[] OriginalVectorData { get; } = new Vector3[10];
    internal List<Ref<Vector3>> InternalVectorData { get; } = new();
    internal Vector3[] InternalSecondaryVectorDataArray { get; set; } = [];
    internal List<Ref<Vector3>> InternalSecondaryVectorData { get; } = new();

    internal Vector3[] OriginalDialogues { get; } = new Vector3[20];
    internal List<Ref<Vector3>> InternalDialogues { get; } = new();

    internal int[] OriginalBattleEnemyIds { get; } = new int[4];
    internal List<Ref<int>> InternalBattleEnemyIds { get; } = new();

    internal Vector3 InternalEmoticonOffset { get; set; } = Vector3.zero;
    internal Vector2[] OriginalEmoticonFlags { get; } = Enumerable.Repeat(new Vector2(-1, 0), 10).ToArray();
    internal List<Ref<Vector2>> InternalEmoticonFlags { get; } = new();

    internal string UnusedOverflowData { get; set; } = "";
}