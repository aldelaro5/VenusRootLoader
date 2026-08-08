using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Properly implement custom languages
[ExposeFromVenus]
public sealed class LanguageLeaf : Leaf
{
    internal LanguageLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }
}