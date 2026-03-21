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
    private readonly ILeavesRegistry<RecipeLeaf> _recipesRegistry;

    public RecipeLibraryEntryTextAssetParser(
        ILeavesRegistry<ItemLeaf> itemsRegistry,
        ILeavesRegistry<RecipeLeaf> recipesRegistry)
    {
        _itemsRegistry = itemsRegistry;
        _recipesRegistry = recipesRegistry;
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
        {
            value.Recipe = new(new(-1, "", ""));
            value.Recipe.Leaf.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(text)]);
            return;
        }

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");

        string[] fields = text.Replace("@", "").Split(StringUtils.CommaSplitDelimiter);
        int firstItem = int.Parse(fields[0]);
        if (firstItem == -1)
        {
            RecipeLeaf incompatibleRecipeLeaf = new(-1, "INCOMPATIBLE", value.CreatorId)
            {
                FirstItem = null,
                SecondItem = null,
                ResultItem = value.Recipe.Leaf.ResultItem
            };
            value.Recipe = new(incompatibleRecipeLeaf);
            return;
        }

        value.Recipe.Leaf.FirstItem = new(_itemsRegistry.LeavesByGameIds[firstItem]);
        if (fields.Length > 1)
            value.Recipe.Leaf.SecondItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[1])]);

        RecipeLeaf foundRecipe = _recipesRegistry.LeavesByNamedIds.Values
            .First(r => r.ResultItem == value.Recipe.Leaf.ResultItem &&
                        ((r.FirstItem == value.Recipe.Leaf.FirstItem &&
                          r.SecondItem == value.Recipe.Leaf.SecondItem) ||
                         (r.FirstItem == value.Recipe.Leaf.SecondItem &&
                          r.SecondItem == value.Recipe.Leaf.FirstItem)));
        value.Recipe = new(foundRecipe);
    }
}