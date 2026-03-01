using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class MenuTextLocalizedTextAssetParser : ILocalizedTextAssetParser<MenuTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MenuTextLeaf leaf)
    {
        string? text = null;
        if (leaf.Text.Count == 0)
            text = "";

        if (leaf.Text.TryGetValue(languageId, out string value))
            text = value;

        if (text != null)
            return text;

        int firstLanguage = leaf.Text.Keys.Min();
        text = leaf.Text[firstLanguage];

        return text;
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MenuTextLeaf leaf) =>
        leaf.Text[languageId] = text;
}