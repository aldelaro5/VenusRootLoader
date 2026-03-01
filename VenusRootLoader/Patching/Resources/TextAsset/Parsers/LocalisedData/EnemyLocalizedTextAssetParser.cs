using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class EnemyLocalizedTextAssetParser : ILocalizedTextAssetParser<EnemyLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, EnemyLeaf leaf)
    {
        EnemyLeaf.EnemyLanguageData? languageData = null;
        if (leaf.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<EnemyLeaf.EnemyLanguageData>();

        if (leaf.LanguageData.TryGetValue(languageId, out EnemyLeaf.EnemyLanguageData value))
            languageData = value;

        if (languageData == null)
        {
            int firstLanguage = leaf.LanguageData.Keys.Min();
            languageData = leaf.LanguageData[firstLanguage];
        }
        
        StringBuilder sb = new();
        sb.Append(languageData.Name);
        sb.Append('@');
        sb.Append(string.Join("{", languageData.PaginatedBiography));
        sb.Append('@');
        sb.Append(languageData.BeeSpyDialogue);
        sb.Append('@');
        sb.Append(languageData.BeetleSpyDialogue);
        sb.Append('@');
        sb.Append(languageData.MothSpyDialogue);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LanguageData[languageId] = new()
        {
            Name = fields[0],
            PaginatedBiography = fields[1].Split(StringUtils.OpeningBraceSplitDelimiter).ToList(),
            BeeSpyDialogue = fields[2],
            BeetleSpyDialogue = fields[3],
            MothSpyDialogue = fields[4]
        };
    }
}