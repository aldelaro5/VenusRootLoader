using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class ItemLocalizedTextAssetParser : ILocalizedTextAssetParser<ItemLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, ItemLeaf leaf)
    {
        ItemLeaf.ItemLanguageData? languageData = null;
        if (leaf.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<ItemLeaf.ItemLanguageData>();

        if (leaf.LanguageData.TryGetValue(languageId, out ItemLeaf.ItemLanguageData value))
            languageData = value;

        if (languageData == null)
        {
            int firstLanguage = leaf.LanguageData.Keys.Min();
            languageData = leaf.LanguageData[firstLanguage];
        }
        
        StringBuilder sb = new();
        sb.Append(languageData.Name);
        sb.Append('@');
        sb.Append(languageData.UnusedDescription);
        sb.Append('@');
        sb.Append(languageData.Description);
        if (languageData.Prepender != null)
        {
            sb.Append('@');
            sb.Append(languageData.Prepender);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, ItemLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LanguageData[languageId] = new();
        leaf.LanguageData[languageId].Name = fields[0];
        leaf.LanguageData[languageId].UnusedDescription = fields[1];
        leaf.LanguageData[languageId].Description = fields[2];
        if (fields.Length > 3)
            leaf.LanguageData[languageId].Prepender = fields[3];
    }
}