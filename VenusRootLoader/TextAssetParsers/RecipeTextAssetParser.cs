using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class RecipeTextAssetParser : ITextAssetParser<RecipeLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RecipeLeaf leaf)
    {
        int secondItem = leaf.SecondItemGameId ?? -1;
        return $"{leaf.FirstItemGameId},{secondItem},{leaf.ResultItemGameId}";
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        leaf.FirstItemGameId = int.Parse(fields[0]);
        int secondItem = int.Parse(fields[1]);
        leaf.SecondItemGameId = secondItem == -1 ? null : secondItem;
        leaf.ResultItemGameId = int.Parse(fields[2]);
    }
}