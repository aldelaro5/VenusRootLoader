using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class MedalLocalizedTextAssetParser : ILocalizedTextAssetParser<MedalLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public MedalLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, MedalLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Prepender);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MedalLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new()
        {
            Name = fields[0],
            Description = fields[1],
            Prepender = fields[2]
        };
    }
}