using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class CrystalBerryLocalizedTextAssetParser : ILocalizedTextAssetParser<CrystalBerryLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, CrystalBerryLeaf leaf)
    {
        string? text = null;
        if (leaf.FortuneTellerHint.Count == 0)
            text = "";

        if (leaf.FortuneTellerHint.TryGetValue(languageId, out string value))
            text = value;

        if (text != null)
            return text;

        int firstLanguage = leaf.FortuneTellerHint.Keys.Min();
        text = leaf.FortuneTellerHint[firstLanguage];

        return text;
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CrystalBerryLeaf leaf) =>
        leaf.FortuneTellerHint[languageId] = text;
}