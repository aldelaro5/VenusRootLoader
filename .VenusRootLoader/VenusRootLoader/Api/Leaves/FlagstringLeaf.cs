using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagstringLeaf : Leaf
{
    internal FlagstringLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }
}