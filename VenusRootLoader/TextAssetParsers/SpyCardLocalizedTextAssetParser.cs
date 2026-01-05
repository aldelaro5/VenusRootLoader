using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class SpyCardLocalizedTextAssetParser : ILocalizedTextAssetSerializable<SpyCardLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, SpyCardLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.Description[languageId]);
        sb.Append('@');
        sb.Append(leaf.HorizontalNameSize[languageId]);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SpyCardLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.Description[languageId] = fields[0];
        leaf.HorizontalNameSize[languageId] = float.Parse(fields[1]);
    }
}