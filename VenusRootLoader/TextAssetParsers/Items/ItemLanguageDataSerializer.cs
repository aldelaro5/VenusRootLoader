using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Items;

internal sealed class ItemLanguageDataSerializer : ILocalizedTextAssetSerializable<ItemLeaf, int>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, ItemLeaf item)
    {
        ItemLeaf.ItemLanguageData? languageData = null;
        if (item.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<ItemLeaf.ItemLanguageData>();

        if (item.LanguageData.TryGetValue(languageId, out ItemLeaf.ItemLanguageData value))
            languageData = value;

        if (languageData == null)
        {
            int firstLanguage = item.LanguageData.Keys.Min();
            languageData = item.LanguageData[firstLanguage];
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

    public void FromTextAssetSerializedString(int languageId, string text, ItemLeaf data)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        data.LanguageData[languageId] = new();
        data.LanguageData[languageId].Name = fields[0];
        data.LanguageData[languageId].UnusedDescription = fields[1];
        data.LanguageData[languageId].Description = fields[2];
        if (fields.Length > 3)
            data.LanguageData[languageId].Prepender = fields[3];
    }
}