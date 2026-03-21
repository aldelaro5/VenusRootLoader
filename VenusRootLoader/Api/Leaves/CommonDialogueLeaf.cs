namespace VenusRootLoader.Api.Leaves;

public sealed class CommonDialogueLeaf : Leaf
{
    internal CommonDialogueLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    internal int InternalGameIndex => Math.Abs(GameId) - 1;

    public LocalizedData<string> LocalizedText { get; } = new();
}