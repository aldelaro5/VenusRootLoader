using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetParser<in T>
    where T : Leaf
{
    string GetTextAssetSerializedString(string subPath, int languageId, T leaf);
    void FromTextAssetSerializedString(string subPath, int languageId, string text, T leaf);
}