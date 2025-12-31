using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.SpyCards;

public sealed class SpyCardLanguageData : ITextAssetSerializable
{
    public string Description { get; set; } = "<NO DESCRIPTION>";
    public float HorizontalNameSize { get; set; } = 1.0f;

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(Description);
        sb.Append('@');
        sb.Append(HorizontalNameSize);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        Description = fields[0];
        HorizontalNameSize = float.Parse(fields[1]);
    }
}