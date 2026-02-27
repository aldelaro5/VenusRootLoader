namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLibraryEntryLeaf : Leaf
{
    public Branch<RecipeLeaf> Recipe { get; set; }
}