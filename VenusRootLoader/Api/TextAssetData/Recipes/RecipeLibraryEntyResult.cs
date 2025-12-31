using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Recipes;

public sealed class RecipeLibraryEntyResult : ITextAssetSerializable
{
    public int ResultItemGameId { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => ResultItemGameId.ToString();
    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => ResultItemGameId = int.Parse(text);
}