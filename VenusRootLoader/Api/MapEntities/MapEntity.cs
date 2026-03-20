using UnityEngine;

namespace VenusRootLoader.Api.MapEntities;

public sealed class MapEntity : IIdentifiable
{
    public int Id { get; init; }
    public string Name { get; set; } = "";
    public NPCControl.NPCType Type { get; set; }
    public NPCControl.ObjectTypes ObjectType { get; set; }
    public NPCControl.ActionBehaviors PrimaryBehavior { get; set; }
    public NPCControl.ActionBehaviors SecondaryBehavior { get; set; }
    public NPCControl.Interaction NpcInteraction { get; set; }
    public NPCControl.DeathType DeathType { get; set; }
    public Vector3 StartingPosition { get; set; }
    public int AnimIdOrItemId { get; set; }
    public bool IsFlipped { get; set; }
    public float CcolHeight { get; set; }
    public float CcolRadius { get; set; }
    public float Radius { get; set; }
    public float Timer { get; set; }
    public float Speed { get; set; }
    public float PrimaryActionFrequency { get; set; }
    public float SecondaryActionFrequency { get; set; }
    public float SpeedMultiplier { get; set; }
    public float RadiusLimit { get; set; }
    public float WanderRadius { get; set; }
    public float TeleportRadius { get; set; }
    public bool HaxBoxCol { get; set; }
    public bool BoxColIsTrigger { get; set; }
    public Vector3 BoxColSize { get; set; } = Vector3.one;
    public Vector3 BoxColCenter { get; set; }
    public float FreezeTime { get; set; }
    public Vector3 FreezeSize { get; set; } = Vector3.one;
    public Vector3 FreezeOffset { get; set; } = Vector3.zero;
    public int EventId { get; set; }
    internal int[] OriginalRequires { get; } = new int[10];
    public List<int> Requires { get; } = new();
    internal int[] OriginalLimits { get; } = new int[10];
    public List<int> Limit { get; } = new();
    internal int[] OriginalData { get; } = new int[10];
    public List<int> Data { get; } = new();
    internal Vector3[] OriginalVectorData { get; } = new Vector3[10];
    public List<Vector3> VectorData { get; } = new();
    internal Vector3[] OriginalDialogues { get; } = new Vector3[20];
    public List<Vector3> Dialogues { get; } = new();
    public Vector3 EulerAngles { get; set; }
    internal int[] OriginalBattleEnemyIds { get; } = new int[4];
    public List<int> BattleEnemyIds { get; } = new();
    public Color TagColor { get; set; }
    public Vector3 EmoticonOffset { get; set; } = Vector3.zero;
    public int InsideId { get; set; }
    internal Vector2[] OriginalEmoticonFlags { get; } = new Vector2[10];
    public List<Vector2> EmoticonFlags { get; } = new();
    public int SpyDialogueMapId { get; set; }
    public int RegionalFlagId { get; set; }
    public float InitialHeight { get; set; }
    public float BobRange { get; set; }
    public float BobSpeed { get; set; }
    public int ActivationFlagId { get; set; }
    internal bool IsReturnToHeightOriginallyInt { get; set; }
    public bool ReturnToHeight { get; set; }
    internal string UnusedOverflowData { get; set; } = "";
}