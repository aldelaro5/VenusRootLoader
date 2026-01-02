using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetData.Miscellaneous;

internal sealed class LetterPrompt : ITextAssetSerializable
{
    internal StringBuilder LetterPromptContentBuilder { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => LetterPromptContentBuilder.ToString();

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        LetterPromptContentBuilder.Clear();
        LetterPromptContentBuilder.Append(text);
    }
}