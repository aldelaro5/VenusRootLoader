using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class PrizeMedalLeaf : Leaf
{
    internal PrizeMedalLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public int MedalGameId { get; set; }
    public int FlagvarGameId { get; set; }
    public int DisplayedEnemyGameId { get; set; }
}