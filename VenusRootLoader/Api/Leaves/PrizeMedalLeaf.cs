namespace VenusRootLoader.Api.Leaves;

public sealed class PrizeMedalLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    public int MedalGameId { get; set; }
    public int FlagvarGameId { get; set; }
    public int DisplayedEnemyGameId { get; set; }
}