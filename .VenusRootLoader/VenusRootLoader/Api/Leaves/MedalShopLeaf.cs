using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus(null, false)]
public sealed class MedalShopLeaf : Leaf
{
    internal MedalShopLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<FlagLeaf> BoughtAllStockFlag { get; internal set; }
    public List<Branch<MedalLeaf>> StartingMedalsStock { get; } = new();
}