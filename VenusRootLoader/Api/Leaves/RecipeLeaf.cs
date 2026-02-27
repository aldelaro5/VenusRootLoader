namespace VenusRootLoader.Api.Leaves;

public sealed class RecipeLeaf : Leaf
{
    public Branch<ItemLeaf>? FirstItem { get; set; }
    public Branch<ItemLeaf>? SecondItem { get; set; }
    public Branch<ItemLeaf> ResultItem { get; set; }
}