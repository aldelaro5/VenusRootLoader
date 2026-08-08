using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class CrystalBerryLocalizedTextAssetParser : ILocalizedTextAssetParser<CrystalBerryLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public CrystalBerryLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, CrystalBerryLeaf leaf) =>
        leaf.LocalizedFortuneTellerHint[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CrystalBerryLeaf leaf) =>
        leaf.LocalizedFortuneTellerHint[_languageRegistry.GetByGameId(languageId)] = text;
}