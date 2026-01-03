using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetSerializable<in T>
    where T : ILeaf
{
    string GetTextAssetSerializedString(string subPath, int languageId, T item);
    void FromTextAssetSerializedString(int languageId, string text, T data);
}