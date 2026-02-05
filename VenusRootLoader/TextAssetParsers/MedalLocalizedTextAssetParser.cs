using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class MedalLocalizedTextAssetParser : ILocalizedTextAssetSerializable<MedalLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MedalLeaf leaf)
    {
        MedalLeaf.MedalLanguageData? languageData = null;
        if (leaf.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<MedalLeaf.MedalLanguageData>();

        if (leaf.LanguageData.TryGetValue(languageId, out MedalLeaf.MedalLanguageData value))
            languageData = value;

        if (languageData == null)
        {
            int firstLanguage = leaf.LanguageData.Keys.Min();
            languageData = leaf.LanguageData[firstLanguage];
        }
        
        StringBuilder sb = new();
        sb.Append(languageData.Name);
        sb.Append('@');
        sb.Append(languageData.Description);
        sb.Append('@');
        sb.Append(languageData.Prepender);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MedalLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LanguageData[languageId] = new()
        {
            Name = fields[0],
            Description = fields[1],
            Prepender = fields[2]
        };
    }
}