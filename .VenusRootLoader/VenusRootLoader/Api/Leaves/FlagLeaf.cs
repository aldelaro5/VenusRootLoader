using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagLeaf : Leaf
{
    internal FlagLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }
}