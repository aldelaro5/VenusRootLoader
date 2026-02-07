using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class QuestLocalizedTextAssetParser : ILocalizedTextAssetParser<QuestLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, QuestLeaf leaf)
        => $"{leaf.Name[languageId]}@{leaf.Description[languageId]}@{leaf.Sender[languageId]}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, QuestLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Name[languageId] = fields[0];
        leaf.Description[languageId] = fields[1];
        leaf.Sender[languageId] = fields[2];
    }
}