using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class PrizeMedalLeaf : Leaf
{
    internal PrizeMedalLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public Branch<MedalLeaf> Medal { get; set; }

    public Branch<FlagvarLeaf> Flagvar { get; set; }

    // TODO: Figure out special cases such as "Explorer Duo"
    public int DisplayedEnemyGameId { get; set; }
}