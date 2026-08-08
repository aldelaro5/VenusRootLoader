using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class ItemLocalizedTextAssetParser : ILocalizedTextAssetParser<ItemLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public ItemLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, ItemLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].UnusedDescription);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description);
        if (leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Prepender != null)
        {
            sb.Append('@');
            sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Prepender);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, ItemLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new();
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name = fields[0];
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].UnusedDescription = fields[1];
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description = fields[2];
        if (fields.Length > 3)
            leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Prepender = fields[3];
    }
}