using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class RecordLocalizedTextAssetParser : ILocalizedTextAssetSerializable<RecordLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, RecordLeaf leaf)
        => $"{leaf.Name[languageId]}@{leaf.Description[languageId]}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, RecordLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Name[languageId] = fields[0];
        leaf.Description[languageId] = fields[1];
    }
}