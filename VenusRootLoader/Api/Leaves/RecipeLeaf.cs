namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    public int FirstItemGameId { get; set; }
    public int? SecondItemGameId { get; set; }
    public int ResultItemGameId { get; set; }
}