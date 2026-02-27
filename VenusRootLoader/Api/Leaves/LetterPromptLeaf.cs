using System.Text;

namespace VenusRootLoader.Api.Leaves;

internal sealed class LetterPromptLeaf : Leaf
{
    internal StringBuilder LetterPromptContentBuilder { get; } = new();
}