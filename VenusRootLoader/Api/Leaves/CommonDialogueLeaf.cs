namespace VenusRootLoader.Api.Leaves;

public sealed class CommonDialogueLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int InternalGameIndex => Math.Abs(GameId) - 1;

    public Dictionary<int, string> Text { get; } = new();
}