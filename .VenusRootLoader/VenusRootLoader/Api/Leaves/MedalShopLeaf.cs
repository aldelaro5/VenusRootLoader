using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(null, false)]
public sealed class MedalShopLeaf : Leaf
{
    internal MedalShopLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public Branch<FlagLeaf> BoughtAllStockFlag { get; internal set; }
    public List<Branch<MedalLeaf>> StartingMedalsStock { get; } = new();
}