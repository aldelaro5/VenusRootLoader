using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

internal sealed class SpyCardLocalizedTextAssetParser : ILocalizedTextAssetParser<SpyCardLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, SpyCardLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Description);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].HorizontalNameSize);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SpyCardLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId].Description = fields[0];
        if (fields.Length > 1)
            leaf.LocalizedData[languageId].HorizontalNameSize = float.Parse(fields[1]);
    }
}