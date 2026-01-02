namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetSerializable<in T>
{
    string GetTextAssetSerializedString(T item);
    void FromTextAssetSerializedString(string text, T data);
}