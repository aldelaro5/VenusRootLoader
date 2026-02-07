using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class RecordLocalizedTextAssetParser : ILocalizedTextAssetParser<RecordLeaf>
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