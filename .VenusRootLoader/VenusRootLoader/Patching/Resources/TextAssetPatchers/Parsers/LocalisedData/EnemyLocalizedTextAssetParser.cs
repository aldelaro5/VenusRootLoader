using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class EnemyLocalizedTextAssetParser : ILocalizedTextAssetParser<EnemyLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public EnemyLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, EnemyLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name);
        sb.Append('@');
        sb.Append(string.Join("{", leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedBiography));
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].BeeSpyDialogue);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].BeetleSpyDialogue);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].MothSpyDialogue);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new()
        {
            Name = fields[0],
            PaginatedBiography = fields[1].Split(StringUtils.OpeningBraceSplitDelimiter).ToList(),
            BeeSpyDialogue = fields[2],
            BeetleSpyDialogue = fields[3],
            MothSpyDialogue = fields[4]
        };
    }
}