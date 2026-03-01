using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class LanguageTextAssetParser : ITextAssetParser<LanguageLeaf>
{
    public string GetTextAssetSerializedString(string subPath, LanguageLeaf leaf) => leaf.HelpText;
    public void FromTextAssetSerializedString(string subPath, string text, LanguageLeaf leaf) => leaf.HelpText = text;
}