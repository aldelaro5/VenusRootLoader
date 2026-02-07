using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameRecipesCollector : IBaseGameCollector
{
    private static readonly string[] RecipesData = Resources.Load<TextAsset>("Data/RecipeData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly ILogger<BaseGameRecipesCollector> _logger;
    private readonly ILeavesRegistry<RecipeLeaf> _recipesRegistry;
    private readonly ITextAssetParser<RecipeLeaf> _recipeTextAssetParser;

    public BaseGameRecipesCollector(
        ILeavesRegistry<RecipeLeaf> recipesRegistry,
        ILogger<BaseGameRecipesCollector> logger,
        ITextAssetParser<RecipeLeaf> recipeTextAssetParser)
    {
        _recipesRegistry = recipesRegistry;
        _logger = logger;
        _recipeTextAssetParser = recipeTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < RecipesData.Length; i++)
        {
            string recipe = RecipesData[i];
            RecipeLeaf recipeLeaf = _recipesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _recipeTextAssetParser.FromTextAssetSerializedString("RecipeData", recipe, recipeLeaf);
        }

        _logger.LogInformation("Collected and registered {RecipesAmount} base game recipes", RecipesData.Length);
    }
}