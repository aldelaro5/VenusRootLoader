namespace VenusRootLoader.Api.Leaves;

internal sealed class RankBonusLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int Rank { get; set; }
    internal int BonusType { get; set; }
    internal int FirstParameterValue { get; set; }
    internal int SecondParameterValue { get; set; }
    internal int ThirdParameterValue { get; set; }
}