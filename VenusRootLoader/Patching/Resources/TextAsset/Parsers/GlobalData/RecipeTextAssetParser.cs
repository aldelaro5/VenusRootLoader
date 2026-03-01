using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class RecipeTextAssetParser : ITextAssetParser<RecipeLeaf>
{
    private readonly ILeavesRegistry<ItemLeaf> _itemsRegistry;

    public RecipeTextAssetParser(ILeavesRegistry<ItemLeaf> itemsRegistry)
    {
        _itemsRegistry = itemsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, RecipeLeaf leaf)
    {
        int secondItem = leaf.SecondItem?.GameId ?? -1;
        return $"{leaf.FirstItem!.Value.GameId},{secondItem},{leaf.ResultItem.GameId}";
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        leaf.FirstItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[0])]);
        int secondItem = int.Parse(fields[1]);
        leaf.SecondItem = secondItem == -1 ? null : new(_itemsRegistry.LeavesByGameIds[secondItem]);
        leaf.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[2])]);
    }
}