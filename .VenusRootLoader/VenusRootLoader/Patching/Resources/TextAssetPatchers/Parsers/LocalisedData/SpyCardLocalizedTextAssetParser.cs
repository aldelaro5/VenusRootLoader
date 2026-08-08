using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class SpyCardLocalizedTextAssetParser : ILocalizedTextAssetParser<SpyCardLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public SpyCardLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, SpyCardLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].HorizontalNameSize);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SpyCardLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description = fields[0];
        if (fields.Length > 1)
            leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].HorizontalNameSize = float.Parse(fields[1]);
    }
}