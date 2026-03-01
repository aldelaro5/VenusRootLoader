using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class EnemyLocalizedTextAssetParser : ILocalizedTextAssetParser<EnemyLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, EnemyLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Name);
        sb.Append('@');
        sb.Append(string.Join("{", leaf.LocalizedData[languageId].PaginatedBiography));
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].BeeSpyDialogue);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].BeetleSpyDialogue);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].MothSpyDialogue);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId] = new()
        {
            Name = fields[0],
            PaginatedBiography = fields[1].Split(StringUtils.OpeningBraceSplitDelimiter).ToList(),
            BeeSpyDialogue = fields[2],
            BeetleSpyDialogue = fields[3],
            MothSpyDialogue = fields[4]
        };
    }
}