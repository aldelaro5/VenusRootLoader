using UnityEngine;
using VenusRootLoader.Api.Common;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public abstract class MapEntity
{
    public int Id { get; internal set; }
    public string Name { get; internal set; } = "";
    public Branch<MapLeaf> Map { get; internal set; }

    protected internal NPCControl.NPCType OriginalType { get; internal set; }
    internal abstract NPCControl.NPCType Type { get; }

    protected internal NPCControl.ObjectTypes OriginalObjectType { get; internal set; }
    internal abstract NPCControl.ObjectTypes ObjectType { get; }

    protected internal NPCControl.Interaction OriginalInteraction { get; internal set; }
    internal abstract NPCControl.Interaction Interaction { get; }

    internal abstract void InitializeFromNew();
    internal abstract void InitializeFromExisting(IRegistryResolver registryResolver);

    internal bool IsReturnToHeightOriginallyInt { get; set; }
    public bool ReturnToHeight { get; set; }
    public int InsideId { get; set; } = -1;
    public Color TagColor { get; set; }

    public NPCControl.ActionBehaviors InternalPrimaryBehavior { get; set; }
    public float InternalPrimaryActionFrequency { get; set; }
    public NPCControl.ActionBehaviors InternalSecondaryBehavior { get; set; }
    public float InternalSecondaryActionFrequency { get; set; }

    public NPCControl.DeathType InternalDeathType { get; set; }
    public int InternalAnimIdOrItemId { get; set; } = -1;
    public bool InternalIsFlipped { get; set; }
    public Vector3 InternalStartingPosition { get; set; }
    public float InitialHeight { get; set; }
    public float InternalBobRange { get; set; }
    public float InternalBobSpeed { get; set; }

    public float InternalCcolHeight { get; set; } = 2f;
    public float InternalCcolRadius { get; set; } = 0.5f;

    public Vector3 InternalEulerAngles { get; set; }
    public float InternalRadius { get; set; }
    public float InternalTimer { get; set; } = -1f;
    public float InternalSpeed { get; set; } = 2.5f;
    public float InternalSpeedMultiplier { get; set; } = 1.0f;
    public float InternalRadiusLimit { get; set; }
    public float InternalWanderRadius { get; set; }
    public float InternalTeleportRadius { get; set; }

    public float InternalFreezeTime { get; set; }
    public Vector3 InternalFreezeSize { get; set; } = Vector3.one;
    public Vector3 InternalFreezeOffset { get; set; } = Vector3.zero;

    public int InternalActivationFlagId { get; set; } = -1;
    public int InternalRegionalFlagId { get; set; } = -1;

    public int InternalSpyDialogueMapId { get; set; }
    public int InternalEventId { get; set; } = -1;

    public bool InternalHaxBoxCol { get; set; }
    public bool InternalBoxColIsTrigger { get; set; }
    public Vector3 InternalBoxColSize { get; set; } = Vector3.one;
    public Vector3 InternalBoxColCenter { get; set; }

    internal int[] OriginalRequires { get; } = new int[10];
    public List<Branch<FlagLeaf>> InternalRequires { get; } = new();
    internal int[] OriginalLimits { get; } = new int[10];
    public List<LimitFlag> InternalLimits { get; } = new();

    internal int[] OriginalData { get; } = new int[10];
    public List<int> InternalData { get; } = new();

    internal Vector3[] OriginalVectorData { get; } = new Vector3[10];
    public List<Vector3> InternalVectorData { get; } = new();
    internal List<Vector3> InternalSecondaryVectorData { get; } = new();
    internal Vector3[] InternalSecondaryVectorDataArray { get; set; } = [];

    internal Vector3[] OriginalDialogues { get; } = new Vector3[20];
    public List<Vector3> InternalDialogues { get; } = new();

    internal int[] OriginalBattleEnemyIds { get; } = new int[4];
    public List<int> InternalBattleEnemyIds { get; } = new();

    public Vector3 InternalEmoticonOffset { get; set; } = Vector3.zero;
    internal Vector2[] OriginalEmoticonFlags { get; } = Enumerable.Repeat(new Vector2(-1, 0), 10).ToArray();
    public List<Vector2> InternalEmoticonFlags { get; } = new();

    internal string UnusedOverflowData { get; set; } = "";
}