using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.Miscellaneous;

internal class Credits : ITextAssetSerializable
{
    internal StringBuilder CreditsTextBuilder { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => CreditsTextBuilder.ToString();

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        CreditsTextBuilder.Clear();
        CreditsTextBuilder.Append(text);
    }
}