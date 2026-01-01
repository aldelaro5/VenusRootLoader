using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Miscellaneous;

public class Credits : ITextAssetSerializable
{
    public StringBuilder CreditsTextBuilder { get; } = new();

    public string GetTextAssetSerializedString() => CreditsTextBuilder.ToString();

    public void FromTextAssetSerializedString(string text)
    {
        CreditsTextBuilder.Clear();
        CreditsTextBuilder.Append(text);
    }
}