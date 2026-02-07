using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class ActionCommandTextAssetParser : ILocalizedTextAssetParser<ActionCommandLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, ActionCommandLeaf leaf) =>
        leaf.Instructions[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, ActionCommandLeaf leaf) =>
        leaf.Instructions[languageId] = text;
}