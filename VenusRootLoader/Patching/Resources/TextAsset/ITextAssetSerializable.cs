using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetSerializable<in T, U>
    where T : ILeaf<U>
{
    string GetTextAssetSerializedString(T item);
    void FromTextAssetSerializedString(string text, T data);
}