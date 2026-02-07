using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class LetterPromptTextAssetParser : ITextAssetParser<LetterPromptLeaf>
{
    public string GetTextAssetSerializedString(string subPath, LetterPromptLeaf leaf)
        => leaf.LetterPromptContentBuilder.ToString();

    public void FromTextAssetSerializedString(string subPath, string text, LetterPromptLeaf leaf)
    {
        leaf.LetterPromptContentBuilder.Clear();
        leaf.LetterPromptContentBuilder.Append(text);
    }
}