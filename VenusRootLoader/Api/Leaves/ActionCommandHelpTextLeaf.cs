namespace VenusRootLoader.Api.Leaves;

internal sealed class ActionCommandHelpTextLeaf : Leaf
{
    internal LocalizedData<string> HelpText { get; } = new();
}