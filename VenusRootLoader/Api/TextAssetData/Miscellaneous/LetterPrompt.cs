using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Miscellaneous;

public sealed class LetterPrompt : ITextAssetSerializable
{
    public StringBuilder LetterPromptContentBuilder { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => LetterPromptContentBuilder.ToString();

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        LetterPromptContentBuilder.Clear();
        LetterPromptContentBuilder.Append(text);
    }
}