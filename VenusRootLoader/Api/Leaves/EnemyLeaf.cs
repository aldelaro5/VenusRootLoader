using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

internal sealed class EnemyLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Name { get; } = new();
    internal Dictionary<int, string> Biography { get; } = new();
    internal Dictionary<int, string> BeeSpyDialogue { get; } = new();
    internal Dictionary<int, string> BeetleSpyDialogue { get; } = new();
    internal Dictionary<int, string> MothSpyDialogue { get; } = new();

    internal int EntityAnimId { get; set; }
    internal int BaseMaxHp { get; set; }
    internal int BaseDefense { get; set; }
    internal int BaseExpReward { get; set; }
    internal int BaseBerriesDrop { get; set; }
    internal Vector3 CursorOffset { get; set; } = Vector3.zero;
    internal int PoisonResistance { get; set; }
    internal int FreezeResistance { get; set; }
    internal int NumbResistance { get; set; }
    internal int SleepResistance { get; set; }
    internal float Size { get; set; }
    internal Vector3 EntityFreezeSize { get; set; } = Vector3.one;
    internal Vector3 EntityFreezeOffset { get; set; } = Vector3.zero;
    internal BattleControl.BattlePosition StartingBattlePosition { get; set; }
    internal float EntityHeight { get; set; }
    internal float EntityBobSpeed { get; set; }
    internal float EntityBobRange { get; set; }
    internal List<BattleControl.AttackProperty> Properties { get; } = new();
    internal float Weight { get; set; }
    internal int BaseEnemyId { get; set; }
    internal int EventIdOnDeath { get; set; }
    internal int ActorTurnAmountPerMainTurn { get; set; }
    internal bool CannotBeTaunted { get; set; }
    internal bool CannotFall { get; set; }
    internal bool HasFixedExpScaling { get; set; }
    internal bool DoesNotHaveExhaustion { get; set; }
    internal bool HasStatsHiddenFromHud { get; set; }
    internal int DeathMethod { get; set; }
    internal List<int> EnemyIdsWhoTriggersHitActionOnHit { get; } = new();
    internal int HardModeAttackIncrease { get; set; }
    internal int HardModeBaseMaxHpIncrease { get; set; }
    internal int HardModeBaseDefenseIncrease { get; set; }
    internal int DefenseIncreaseWhenDefending { get; set; }
    internal Vector3 ItemOffset { get; set; } = Vector3.zero;
    internal bool UseBattleIdleAsEntityBaseState { get; set; }
    internal int EnemyPortraitsSpriteIndex { get; set; }
    internal bool CannotBeSpied { get; set; }
    internal int EventIdOnFall { get; set; }
    internal int HitActionTrigger { get; set; }
    internal bool CanActWhileStunned { get; set; }
    internal float SizeOnFreeze { get; set; }
}