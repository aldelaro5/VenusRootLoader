using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class CommonDialogueLeaf : DialogueLeaf
{
    internal CommonDialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }
    internal override Branch<MapLeaf>? AssociatedMap => null;

    internal int InternalGameIndex => Math.Abs(GameId) - 1;

    public LocalizedData<string> LocalizedText { get; } = new();
}