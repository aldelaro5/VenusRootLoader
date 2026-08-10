using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class RecipeLeaf : Leaf
{
    internal RecipeLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<ItemLeaf>? FirstItem { get; set; }
    public Branch<ItemLeaf>? SecondItem { get; set; }
    public Branch<ItemLeaf> ResultItem { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        Branch<ItemLeaf> firstItem,
        Branch<ItemLeaf>? secondItem,
        Branch<ItemLeaf> resultItem)
    {
        FirstItem = firstItem;
        SecondItem = secondItem;
        ResultItem = resultItem;
    }
}