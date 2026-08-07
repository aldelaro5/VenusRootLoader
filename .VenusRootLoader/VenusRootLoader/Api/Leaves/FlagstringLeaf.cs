using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FlagstringLeaf : Leaf
{
    internal FlagstringLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }
}