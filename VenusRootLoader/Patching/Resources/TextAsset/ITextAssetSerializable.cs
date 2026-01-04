using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetSerializable<in T>
    where T : ILeaf
{
    string GetTextAssetSerializedString(string subPath, T leaf);
    void FromTextAssetSerializedString(string subPath, string text, T leaf);
}