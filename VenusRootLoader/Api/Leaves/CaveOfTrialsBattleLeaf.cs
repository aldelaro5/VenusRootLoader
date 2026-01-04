namespace VenusRootLoader.Api.Leaves;

internal sealed class CaveOfTrialsBattleLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal List<int> EnemyIdsInBattle { get; } = new();
}