using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

internal interface ITextAssetParser<in T>
    where T : Leaf
{
    string GetTextAssetSerializedString(string subPath, T value);
    void FromTextAssetSerializedString(string subPath, string text, T value);
}