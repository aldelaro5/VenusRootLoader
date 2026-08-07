using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class RecipeLibraryEntryLeaf : Leaf
{
    internal RecipeLibraryEntryLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal bool OriginalEndsWithAtSymbol { get; set; }
    internal bool OriginalItemsHaveInvertedOrder { get; set; }
    public Branch<RecipeLeaf> Recipe { get; set; }
}