using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.Recipes;

internal sealed class RecipeLibraryEntyResult : ITextAssetSerializable
{
    internal int ResultItemGameId { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => ResultItemGameId.ToString();
    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => ResultItemGameId = int.Parse(text);
}