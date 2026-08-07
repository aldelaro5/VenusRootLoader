using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class SpyCardsTextLeaf : Leaf
{
    internal SpyCardsTextLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}