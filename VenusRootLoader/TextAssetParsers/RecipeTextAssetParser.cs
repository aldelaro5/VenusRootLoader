using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class RecipeTextAssetParser : ITextAssetSerializable<RecipeLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RecipeLeaf leaf)
        => $"{leaf.FirstItemGameId},{leaf.SecondItemGameId},{leaf.ResultItemGameId}";

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        leaf.FirstItemGameId = int.Parse(fields[0]);
        leaf.SecondItemGameId = int.Parse(fields[1]);
        leaf.ResultItemGameId = int.Parse(fields[2]);
    }
}