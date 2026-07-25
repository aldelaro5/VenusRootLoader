using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class EventLeaf : Leaf
{
    internal EventLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }
}