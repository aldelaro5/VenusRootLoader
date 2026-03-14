using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface ITextAssetParser<in T>
    where T : Leaf
{
    string GetTextAssetSerializedString(string subPath, T leaf);
    void FromTextAssetSerializedString(string subPath, string text, T leaf);
}