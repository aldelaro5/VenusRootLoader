using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Items;

internal sealed class ItemLanguageDataSerializer : ITextAssetSerializable<ItemLeaf.ItemLanguageData>
{
    public string GetTextAssetSerializedString(ItemLeaf.ItemLanguageData itemLanguageData)
    {
        StringBuilder sb = new();
        sb.Append(itemLanguageData.Name);
        sb.Append('@');
        sb.Append(itemLanguageData.UnusedDescription);
        sb.Append('@');
        sb.Append(itemLanguageData.Description);
        if (itemLanguageData.Prepender != null)
        {
            sb.Append('@');
            sb.Append(itemLanguageData.Prepender);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string text, ItemLeaf.ItemLanguageData data)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        data.Name = fields[0];
        data.UnusedDescription = fields[1];
        data.Description = fields[2];
        if (fields.Length > 3)
            data.Prepender = fields[3];
    }
}