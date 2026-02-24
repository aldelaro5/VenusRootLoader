using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class RecordLocalizedTextAssetParser : ILocalizedTextAssetParser<RecordLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, RecordLeaf leaf)
    {
        RecordLeaf.RecordLanguageData? languageData = null;
        if (leaf.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<RecordLeaf.RecordLanguageData>();

        if (leaf.LanguageData.TryGetValue(languageId, out RecordLeaf.RecordLanguageData value))
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

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, RecordLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LanguageData[languageId] = new()
        {
            Name = fields[0],
            Description = fields[1]
        };
    }
}