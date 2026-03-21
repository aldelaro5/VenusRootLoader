namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLibraryEntryLeaf : Leaf
{
    internal RecipeLibraryEntryLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public Branch<RecipeLeaf> Recipe { get; set; }
}