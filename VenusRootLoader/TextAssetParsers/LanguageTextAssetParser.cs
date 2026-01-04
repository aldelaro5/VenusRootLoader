using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class LanguageTextAssetParser : ITextAssetSerializable<LanguageLeaf>
{
    public string GetTextAssetSerializedString(string subPath, LanguageLeaf leaf) => leaf.HelpText;
    public void FromTextAssetSerializedString(string subPath, string text, LanguageLeaf leaf) => leaf.HelpText = text;
}