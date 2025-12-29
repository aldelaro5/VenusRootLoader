namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetSerializable
{
    string GetTextAssetSerializedString();
    void FromTextAssetSerializedString(string text);
}