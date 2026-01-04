using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class MedalLocalizedTextAssetParser : ILocalizedTextAssetSerializable<MedalLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MedalLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.Name[languageId]);
        sb.Append('@');
        sb.Append(leaf.Description[languageId]);
        sb.Append('@');
        sb.Append(leaf.Prepender[languageId]);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MedalLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.Name[languageId] = fields[0];
        leaf.Description[languageId] = fields[1];
        leaf.Prepender[languageId] = fields[2];
    }
}