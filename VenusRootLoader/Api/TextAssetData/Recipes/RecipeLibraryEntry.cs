using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Recipes;

public sealed class RecipeLibraryEntry : ITextAssetSerializable
{
    private List<int> ItemGameIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        string[] itemIds = ItemGameIds.Select(i => i.ToString()).ToArray();
        sb.Append(string.Join(",", itemIds));
        sb.Append('@');
        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] itemIds = text.Remove('@').Split(StringUtils.CommaSplitDelimiter);
        ItemGameIds.Clear();
        foreach (string itemId in itemIds)
            ItemGameIds.Add(int.Parse(itemId));
    }
}