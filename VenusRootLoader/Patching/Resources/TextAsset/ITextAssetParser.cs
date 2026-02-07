using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetParser<in T>
    where T : ILeaf
{
    string GetTextAssetSerializedString(string subPath, T leaf);
    void FromTextAssetSerializedString(string subPath, string text, T leaf);
}