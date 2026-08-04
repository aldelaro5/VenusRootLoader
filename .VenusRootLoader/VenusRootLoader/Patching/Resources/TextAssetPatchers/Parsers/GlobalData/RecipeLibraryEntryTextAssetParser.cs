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
            return leaf.Recipe.Resolve().ResultItem.Resolve().GameId.ToString();

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        StringBuilder sb = new();
        // This is the special incompatible recipe.
        if (leaf.Recipe.Resolve().FirstItem is null &&
            leaf.Recipe.Resolve().SecondItem is null)
        {
            sb.Append("-1@");
            return sb.ToString();
        }

        if (leaf.Recipe.Resolve().FirstItem is not null && leaf.Recipe.Resolve().SecondItem is not null)
        {
            sb.Append(
                leaf.OriginalItemsHaveInvertedOrder
                    ? leaf.Recipe.Resolve().SecondItem!.Value.Resolve().GameId
                    : leaf.Recipe.Resolve().FirstItem!.Value.Resolve().GameId);
            sb.Append(',');
            sb.Append(
                leaf.OriginalItemsHaveInvertedOrder
                    ? leaf.Recipe.Resolve().FirstItem!.Value.Resolve().GameId
                    : leaf.Recipe.Resolve().SecondItem!.Value.Resolve().GameId);
        }
        else
        {
            sb.Append(leaf.Recipe.Resolve().FirstItem!.Value.Resolve().GameId);
        }

        if (leaf.OriginalEndsWithAtSymbol)
            sb.Append('@');
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
        {
            // We assume this will be read first so we need to have a blank leaf to receive the other TextAsset info
            // before we can fully resolve it.
            leaf.Recipe = new(new(-1, "", ""));
            leaf.Recipe.Resolve().ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(text)]);
            return;
        }

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");

        leaf.OriginalEndsWithAtSymbol = text.EndsWith("@");
        string[] fields = text.Replace("@", "").Split(StringUtils.CommaSplitDelimiter);
        int firstItem = int.Parse(fields[0]);
        if (firstItem == -1)
        {
            // This RecipeLeaf is special because it can't really exist in the registry so it has to be a placeholder
            // since you can still edit the result item.
            RecipeLeaf incompatibleRecipeLeaf = new(-1, "INCOMPATIBLE", leaf.CreatorId)
            {
                FirstItem = null,
                SecondItem = null,
                ResultItem = leaf.Recipe.Resolve().ResultItem
            };
            leaf.Recipe = new(incompatibleRecipeLeaf);
            return;
        }

        leaf.Recipe.Resolve().FirstItem = new(_itemsRegistry.LeavesByGameIds[firstItem]);
        if (fields.Length > 1)
            leaf.Recipe.Resolve().SecondItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[1])]);

        RecipeLeaf foundRecipe = _recipesRegistry.LeavesByEffectiveIds.Values
            .First(r => r.ResultItem == leaf.Recipe.Resolve().ResultItem &&
                        ((r.FirstItem == leaf.Recipe.Resolve().FirstItem &&
                          r.SecondItem == leaf.Recipe.Resolve().SecondItem) ||
                         (r.FirstItem == leaf.Recipe.Resolve().SecondItem &&
                          r.SecondItem == leaf.Recipe.Resolve().FirstItem)));
        leaf.OriginalItemsHaveInvertedOrder = foundRecipe.FirstItem == leaf.Recipe.Resolve().SecondItem &&
                                              foundRecipe.SecondItem == leaf.Recipe.Resolve().FirstItem;
        leaf.Recipe = new(foundRecipe);
    }
}