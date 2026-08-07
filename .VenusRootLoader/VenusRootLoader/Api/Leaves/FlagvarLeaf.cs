using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagvarLeaf : Leaf
{
    internal FlagvarLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }
}