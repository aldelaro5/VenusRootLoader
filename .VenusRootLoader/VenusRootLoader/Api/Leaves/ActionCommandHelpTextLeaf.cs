namespace VenusRootLoader.Api.Leaves;

internal sealed class ActionCommandHelpTextLeaf : Leaf
{
    internal ActionCommandHelpTextLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal LocalizedData<string> HelpText { get; } = new();
}