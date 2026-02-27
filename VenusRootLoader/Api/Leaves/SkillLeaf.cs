namespace VenusRootLoader.Api.Leaves;

internal sealed class SkillLeaf : Leaf
{
    internal Dictionary<int, string> Name { get; set; } = new();
    internal Dictionary<int, string> Description { get; set; } = new();

    internal int Cost { get; set; }
    internal BattleControl.AttackArea Target { get; set; }
    internal bool UsableByBee { get; set; }
    internal bool UsableByBeetle { get; set; }
    internal bool UsableByMoth { get; set; }
    internal bool TargetsOnlyGroundedEnemies { get; set; }
    internal bool TargetsOnlyFrontEnemy { get; set; }
    internal int ActionCommandId { get; set; }
    internal bool TargetsAliveOnly { get; set; }
    internal bool IsUserExcludedFromTarget { get; set; }
    internal bool TargetFaintedOnly { get; set; }
}