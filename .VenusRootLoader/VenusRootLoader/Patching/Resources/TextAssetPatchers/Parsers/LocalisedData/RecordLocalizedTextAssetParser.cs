using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class RecordLocalizedTextAssetParser : ILocalizedTextAssetParser<RecordLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public RecordLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, RecordLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, RecordLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new()
        {
            Name = fields[0],
            Description = fields[1]
        };
    }
}