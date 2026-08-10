using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class MedalShopLeaf : Leaf
{
    internal MedalShopLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<FlagLeaf> BoughtAllStockFlag { get; set; } = null!;
    public List<Branch<MedalLeaf>> StartingMedalsStock { get; } = new();

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        Branch<FlagLeaf> boughtAllStockFlag,
        ICollection<Branch<MedalLeaf>> startingMedalsStock)
    {
        BoughtAllStockFlag = boughtAllStockFlag;
        StartingMedalsStock.AddRange(startingMedalsStock);
    }
}