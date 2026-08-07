using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class MenuTextLeaf : Leaf
{
    internal MenuTextLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}