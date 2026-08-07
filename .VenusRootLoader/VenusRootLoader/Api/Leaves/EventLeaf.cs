using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class EventLeaf : Leaf
{
    internal EventLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }
}