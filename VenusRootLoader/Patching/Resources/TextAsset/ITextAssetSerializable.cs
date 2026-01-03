using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetSerializable<in T>
    where T : ILeaf
{
    string GetTextAssetSerializedString(string subPath, T item);
    void FromTextAssetSerializedString(string text, T data);
}