namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLibraryEntryLeaf : Leaf
{
    internal RecipeLibraryEntryLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal bool OriginalEndsWithAtSymbol { get; set; }
    internal bool OriginalItemsHaveInvertedOrder { get; set; }
    public Branch<RecipeLeaf> Recipe { get; set; }
}