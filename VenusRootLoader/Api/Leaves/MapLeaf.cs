using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

// TODO: This API needs several improvements before it's ready
// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
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
        internal int RequiresLength { get; set; }
        public int[] Requires { get; } = new int[10];
        internal int LimitsLength { get; set; }
        public int[] Limit { get; } = new int[10];
        internal int DataLength { get; set; }
        public int[] Data { get; } = new int[10];
        internal int VectorDataLength { get; set; }
        public Vector3[] VectorData { get; } = new Vector3[10];
        internal int DialoguesLength { get; set; }
        public Vector3[] Dialogues { get; } = new Vector3[20];
        public Vector3 EulerAngles { get; set; }
        internal int BattleEnemyIdsLength { get; set; }
        public int[] BattleEnemyIds { get; } = new int[4];
        public Color TagColor { get; set; }
        public Vector3 EmoticonOffset { get; set; } = Vector3.zero;
        public int InsideId { get; set; }
        public Vector2[] EmoticonFlags { get; } = new Vector2[10];
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

    public ReadOnlyListWithCreate<MapEntity> Entities { get; } = new();
    public LocalizedData<List<string>> Dialogues { get; } = new();
}