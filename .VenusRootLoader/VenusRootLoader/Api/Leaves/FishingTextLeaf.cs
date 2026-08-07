using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class FishingTextLeaf : Leaf
{
    internal FishingTextLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}