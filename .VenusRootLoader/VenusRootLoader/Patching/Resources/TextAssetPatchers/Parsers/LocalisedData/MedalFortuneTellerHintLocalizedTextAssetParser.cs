using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class
    MedalFortuneTellerHintLocalizedTextAssetParser : ILocalizedTextAssetParser<MedalFortuneTellerHintLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public MedalFortuneTellerHintLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, MedalFortuneTellerHintLeaf leaf) =>
        leaf.LocalizedHintText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(
        string subPath,
        int languageId,
        string text,
        MedalFortuneTellerHintLeaf leaf) =>
        leaf.LocalizedHintText[_languageRegistry.GetByGameId(languageId)] = text;
}