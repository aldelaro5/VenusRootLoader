using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class RecordLocalizedTextAssetParser : ILocalizedTextAssetParser<RecordLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, RecordLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].Description);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, RecordLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId] = new()
        {
            Name = fields[0],
            Description = fields[1]
        };
    }
}