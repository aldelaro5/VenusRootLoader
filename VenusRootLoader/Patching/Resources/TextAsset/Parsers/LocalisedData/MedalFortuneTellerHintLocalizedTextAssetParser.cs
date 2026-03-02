using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class
    MedalFortuneTellerHintLocalizedTextAssetParser : ILocalizedTextAssetParser<MedalFortuneTellerHintLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MedalFortuneTellerHintLeaf leaf) =>
        leaf.LocalizedHintText[languageId];

    public void FromTextAssetSerializedString(
        string subPath,
        int languageId,
        string text,
        MedalFortuneTellerHintLeaf leaf) =>
        leaf.LocalizedHintText[languageId] = text;
}