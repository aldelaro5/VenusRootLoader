using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class CrystalBerryLocalizedTextAssetParser : ILocalizedTextAssetParser<CrystalBerryLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, CrystalBerryLeaf leaf) =>
        leaf.LocalizedFortuneTellerHint[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CrystalBerryLeaf leaf) =>
        leaf.LocalizedFortuneTellerHint[languageId] = text;
}