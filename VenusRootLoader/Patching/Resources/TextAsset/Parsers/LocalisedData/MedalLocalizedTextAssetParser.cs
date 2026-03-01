using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class MedalLocalizedTextAssetParser : ILocalizedTextAssetParser<MedalLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MedalLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Name);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].Description);
        sb.Append('@');
        sb.Append(leaf.LocalizedData[languageId].Prepender);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MedalLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId] = new()
        {
            Name = fields[0],
            Description = fields[1],
            Prepender = fields[2]
        };
    }
}