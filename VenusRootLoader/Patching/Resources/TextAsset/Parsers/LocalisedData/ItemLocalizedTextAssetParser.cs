using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class ItemLocalizedTextAssetParser : ILocalizedTextAssetParser<ItemLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, ItemLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].UnusedDescription);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].Description);
        if (leaf.LocalizedData[languageId].Prepender != null)
        {
            sb.Append('@');
            sb.Append(leaf.LocalizedData[languageId].Prepender);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, ItemLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId] = new();
        leaf.LocalizedData[languageId].Name = fields[0];
        leaf.LocalizedData[languageId].UnusedDescription = fields[1];
        leaf.LocalizedData[languageId].Description = fields[2];
        if (fields.Length > 3)
            leaf.LocalizedData[languageId].Prepender = fields[3];
    }
}