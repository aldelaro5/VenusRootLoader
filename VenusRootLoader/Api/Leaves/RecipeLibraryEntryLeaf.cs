namespace VenusRootLoader.Api.Leaves;

internal sealed class RecipeLibraryEntryLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int FirstItemGameId { get; set; }
    internal int? SecondItemGameId { get; set; }
    internal int ResultItemGameId { get; set; }
}