using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class RecipeTextAssetParser : ITextAssetParser<RecipeLeaf>
{
    private readonly ILeavesRegistry<ItemLeaf> _itemsRegistry;

    public RecipeTextAssetParser(ILeavesRegistry<ItemLeaf> itemsRegistry)
    {
        _itemsRegistry = itemsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, RecipeLeaf value)
    {
        int secondItem = value.SecondItem?.GameId ?? -1;
        return $"{value.FirstItem!.Value.GameId},{secondItem},{value.ResultItem.GameId}";
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLeaf value)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        value.FirstItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[0])]);
        int secondItem = int.Parse(fields[1]);
        value.SecondItem = secondItem == -1 ? null : new(_itemsRegistry.LeavesByGameIds[secondItem]);
        value.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[2])]);
    }
}