using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class RecipeLibraryEntryTextAssetParser : ITextAssetParser<RecipeLibraryEntryLeaf>
{
    private const string CookOrderSubPath = "CookOrder";
    private const string CookLibrarySubPath = "CookLibrary";

    private readonly ILeavesRegistry<ItemLeaf> _itemsRegistry;

    public RecipeLibraryEntryTextAssetParser(ILeavesRegistry<ItemLeaf> itemsRegistry)
    {
        _itemsRegistry = itemsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, RecipeLibraryEntryLeaf value)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            return value.Recipe.Leaf.ResultItem.GameId.ToString();

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        StringBuilder sb = new();
        if (value.Recipe.Leaf.FirstItem is null &&
            value.Recipe.Leaf.SecondItem is null)
        {
            sb.Append("-1@");
            return sb.ToString();
        }

        sb.Append(value.Recipe.Leaf.FirstItem!.Value.GameId);
        if (value.Recipe.Leaf.SecondItem is not null)
        {
            sb.Append(',');
            sb.Append(value.Recipe.Leaf.SecondItem.Value.GameId);
        }

        sb.Append('@');
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLibraryEntryLeaf value)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            value.Recipe.Leaf.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(text)]);
        else if (subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fields = text.Replace("@", "").Split(StringUtils.CommaSplitDelimiter);
            int firstItem = int.Parse(fields[0]);
            if (firstItem == -1)
            {
                value.Recipe.Leaf.FirstItem = null;
                value.Recipe.Leaf.SecondItem = null;
                return;
            }

            value.Recipe.Leaf.FirstItem = new(_itemsRegistry.LeavesByGameIds[firstItem]);
            if (fields.Length > 1)
                value.Recipe.Leaf.SecondItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[1])]);
        }
        else
        {
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");
        }
    }
}