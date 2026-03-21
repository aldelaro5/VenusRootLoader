namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLeaf : Leaf
{
    internal RecipeLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public Branch<ItemLeaf>? FirstItem { get; set; }
    public Branch<ItemLeaf>? SecondItem { get; set; }
    public Branch<ItemLeaf> ResultItem { get; set; }
}