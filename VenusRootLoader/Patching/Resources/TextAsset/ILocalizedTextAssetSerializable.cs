using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetSerializable<in T, U>
    where T : ILeaf<U>
{
    string GetTextAssetSerializedString(string subPath, int languageId, T item);
    void FromTextAssetSerializedString(int languageId, string text, T data);
}