namespace VenusRootLoader.Api.Leaves;

internal sealed class ActionCommandHelpTextLeaf : Leaf
{
    internal ActionCommandHelpTextLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal LocalizedData<string> HelpText { get; } = new();
}