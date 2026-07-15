using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagvarLeaf : Leaf
{
    internal FlagvarLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }
}