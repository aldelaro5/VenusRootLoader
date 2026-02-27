namespace VenusRootLoader.Api.Leaves;

public sealed class CommonDialogueLeaf : Leaf
{
    internal int InternalGameIndex => Math.Abs(GameId) - 1;

    public Dictionary<int, string> Text { get; } = new();
}