using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
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

    public string GetTextAssetSerializedString(string subPath, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            return leaf.Recipe.Leaf.ResultItem.GameId.ToString();

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        StringBuilder sb = new();
        if (leaf.Recipe.Leaf.FirstItem is null &&
            leaf.Recipe.Leaf.SecondItem is null)
        {
            sb.Append("-1@");
            return sb.ToString();
        }

        if (leaf.Recipe.Leaf.FirstItem is not null && leaf.Recipe.Leaf.SecondItem is not null)
        {
            sb.Append(
                leaf.OriginalItemsHaveInvertedOrder
                    ? leaf.Recipe.Leaf.SecondItem.Value.GameId
                    : leaf.Recipe.Leaf.FirstItem.Value.GameId);
            sb.Append(',');
            sb.Append(
                leaf.OriginalItemsHaveInvertedOrder
                    ? leaf.Recipe.Leaf.FirstItem.Value.GameId
                    : leaf.Recipe.Leaf.SecondItem.Value.GameId);
        }
        else
        {
            sb.Append(leaf.Recipe.Leaf.FirstItem!.Value.GameId);
        }

        if (leaf.OriginalEndsWithAtSymbol)
            sb.Append('@');
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
        {
            leaf.Recipe = new(new(-1, "", ""));
            leaf.Recipe.Leaf.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(text)]);
            return;
        }

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");

        leaf.OriginalEndsWithAtSymbol = text.EndsWith("@");
        string[] fields = text.Replace("@", "").Split(StringUtils.CommaSplitDelimiter);
        int firstItem = int.Parse(fields[0]);
        if (firstItem == -1)
        {
            RecipeLeaf incompatibleRecipeLeaf = new(-1, "INCOMPATIBLE", leaf.CreatorId)
            {
                FirstItem = null,
                SecondItem = null,
                ResultItem = leaf.Recipe.Leaf.ResultItem
            };
            leaf.Recipe = new(incompatibleRecipeLeaf);
            return;
        }

        leaf.Recipe.Leaf.FirstItem = new(_itemsRegistry.LeavesByGameIds[firstItem]);
        if (fields.Length > 1)
            leaf.Recipe.Leaf.SecondItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[1])]);

        RecipeLeaf foundRecipe = _recipesRegistry.LeavesByNamedIds.Values
            .First(r => r.ResultItem == leaf.Recipe.Leaf.ResultItem &&
                        ((r.FirstItem == leaf.Recipe.Leaf.FirstItem &&
                          r.SecondItem == leaf.Recipe.Leaf.SecondItem) ||
                         (r.FirstItem == leaf.Recipe.Leaf.SecondItem &&
                          r.SecondItem == leaf.Recipe.Leaf.FirstItem)));
        leaf.OriginalItemsHaveInvertedOrder = foundRecipe.FirstItem == leaf.Recipe.Leaf.SecondItem &&
                                              foundRecipe.SecondItem == leaf.Recipe.Leaf.FirstItem;
        leaf.Recipe = new(foundRecipe);
    }
}