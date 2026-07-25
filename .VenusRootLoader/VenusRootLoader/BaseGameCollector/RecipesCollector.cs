using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RecipesCollector : IBaseGameCollector
{
    private readonly string[] _recipesData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataRecipesPath);

    private readonly ILogger<RecipesCollector> _logger;
    private readonly ILeavesRegistry<RecipeLeaf> _recipesRegistry;
    private readonly ITextAssetParser<RecipeLeaf> _recipeTextAssetParser;

    public RecipesCollector(
        ILeavesRegistry<RecipeLeaf> recipesRegistry,
        ILogger<RecipesCollector> logger,
        ITextAssetParser<RecipeLeaf> recipeTextAssetParser)
    {
        _recipesRegistry = recipesRegistry;
        _logger = logger;
        _recipeTextAssetParser = recipeTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _recipesData.Length; i++)
        {
            string recipe = _recipesData[i];
            RecipeLeaf recipeLeaf = _recipesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _recipeTextAssetParser.FromTextAssetSerializedString(TextAssetPaths.DataRecipesPath, recipe, recipeLeaf);
        }

        _logger.LogInformation("Collected and registered {RecipesAmount} base game recipes", _recipesData.Length);
    }
}