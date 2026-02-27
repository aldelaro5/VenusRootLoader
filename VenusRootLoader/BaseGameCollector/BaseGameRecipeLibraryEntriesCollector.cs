using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameRecipeLibraryEntriesCollector : IBaseGameCollector
{
    private static readonly string[] CookOrderData = Resources.Load<TextAsset>("Data/CookOrder").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly string[] CookLibraryData = Resources.Load<TextAsset>("Data/CookLibrary").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly ILogger<BaseGameRecipeLibraryEntriesCollector> _logger;
    private readonly ILeavesRegistry<RecipeLeaf> _recipesRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesRegistry;
    private readonly ITextAssetParser<RecipeLibraryEntryLeaf> _recipeTextAssetParser;

    public BaseGameRecipeLibraryEntriesCollector(
        ILogger<BaseGameRecipeLibraryEntriesCollector> logger,
        ILeavesRegistry<RecipeLeaf> recipesRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesRegistry,
        ITextAssetParser<RecipeLibraryEntryLeaf> recipeTextAssetParser)
    {
        _logger = logger;
        _recipesRegistry = recipesRegistry;
        _recipeLibraryEntriesRegistry = recipeLibraryEntriesRegistry;
        _recipeTextAssetParser = recipeTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < CookOrderData.Length; i++)
        {
            string cookLibraryLine = CookLibraryData[i];
            string cookOrderLine = CookOrderData[i];
            RecipeLibraryEntryLeaf recipeData = new();
            recipeData.Recipe = new Branch<RecipeLeaf>(new());
            _recipeTextAssetParser.FromTextAssetSerializedString("CookOrder", cookOrderLine, recipeData);
            _recipeTextAssetParser.FromTextAssetSerializedString(
                "CookLibrary",
                cookLibraryLine,
                recipeData);

            RecipeLeaf foundRecipe;
            if (recipeData.Recipe.Leaf.FirstItem == null &&
                recipeData.Recipe.Leaf.SecondItem == null)
            {
                foundRecipe = new()
                {
                    GameId = -1,
                    NamedId = "INCOMPATIBLE",
                    CreatorId = baseGameId,
                    FirstItem = null,
                    SecondItem = null,
                    ResultItem = recipeData.Recipe.Leaf.ResultItem
                };
            }
            else
            {
                foundRecipe = _recipesRegistry.LeavesByNamedIds.Values
                    .First(r => r.ResultItem == recipeData.Recipe.Leaf.ResultItem &&
                                ((r.FirstItem == recipeData.Recipe.Leaf.FirstItem &&
                                  r.SecondItem == recipeData.Recipe.Leaf.SecondItem) ||
                                 (r.FirstItem == recipeData.Recipe.Leaf.SecondItem &&
                                  r.SecondItem == recipeData.Recipe.Leaf.FirstItem)));
            }

            RecipeLibraryEntryLeaf recipeLibraryEntryLeaf =
                _recipeLibraryEntriesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            recipeLibraryEntryLeaf.Recipe = new(foundRecipe);
        }

        _logger.LogInformation(
            "Collected and registered {RecipeLibraryEntriesAmount} base game recipe library entries",
            CookOrderData.Length);
    }
}