using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class SkillActionCommandHelpTextLeaf : Leaf
{
    internal SkillActionCommandHelpTextLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public LocalizedData<string> HelpText { get; } = new();
}