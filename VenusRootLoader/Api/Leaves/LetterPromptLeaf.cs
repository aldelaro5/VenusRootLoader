using System.Text;

namespace VenusRootLoader.Api.Leaves;

internal sealed class LetterPromptLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal StringBuilder LetterPromptContentBuilder { get; } = new();
}