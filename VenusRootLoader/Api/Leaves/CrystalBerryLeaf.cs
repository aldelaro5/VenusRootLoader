namespace VenusRootLoader.Api.Leaves;

public sealed class CrystalBerryLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    public Dictionary<int, string> FortuneTellerHint { get; } = new();
}