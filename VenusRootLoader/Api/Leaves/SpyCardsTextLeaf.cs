using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class SpyCardsTextLeaf : Leaf
{
    internal SpyCardsTextLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}