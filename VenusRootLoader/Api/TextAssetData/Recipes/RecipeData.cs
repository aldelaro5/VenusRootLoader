using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Recipes;

public sealed class RecipeData : ITextAssetSerializable
{
    public int FirstItemGameId { get; set; }
    public int SecondItemGameId { get; set; } = -1;
    public int ResultItemGameId { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{FirstItemGameId},{SecondItemGameId},{ResultItemGameId}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        FirstItemGameId = int.Parse(fields[0]);
        SecondItemGameId = int.Parse(fields[1]);
        ResultItemGameId = int.Parse(fields[2]);
    }
}