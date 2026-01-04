namespace VenusRootLoader.Api.Leaves;

internal sealed class LanguageLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal string HelpText { get; set; } = "";
}