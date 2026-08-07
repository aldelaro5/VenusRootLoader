using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagLeaf : Leaf
{
    internal FlagLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }
}