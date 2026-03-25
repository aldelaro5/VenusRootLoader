using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RecipeLibraryEntriesCollector : IBaseGameCollector
{
    private static readonly string[] CookOrderData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataRecipesLibraryEntriesResultItemsPath);

    private static readonly string[] CookLibraryData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataRecipesLibraryEntriesInputItemsPath);

    private readonly ILogger<RecipeLibraryEntriesCollector> _logger;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesRegistry;
    private readonly ITextAssetParser<RecipeLibraryEntryLeaf> _recipeTextAssetParser;

    public RecipeLibraryEntriesCollector(
        ILogger<RecipeLibraryEntriesCollector> logger,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesRegistry,
        ITextAssetParser<RecipeLibraryEntryLeaf> recipeTextAssetParser)
    {
        _logger = logger;
        _recipeLibraryEntriesRegistry = recipeLibraryEntriesRegistry;
        _recipeTextAssetParser = recipeTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < CookOrderData.Length; i++)
        {
            string cookLibraryLine = CookLibraryData[i];
            string cookOrderLine = CookOrderData[i];
            RecipeLibraryEntryLeaf recipeLibraryEntryLeaf =
                _recipeLibraryEntriesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _recipeTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataRecipesLibraryEntriesResultItemsPath,
                cookOrderLine,
                recipeLibraryEntryLeaf);
            _recipeTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataRecipesLibraryEntriesInputItemsPath,
                cookLibraryLine,
                recipeLibraryEntryLeaf);
        }

        _logger.LogInformation(
            "Collected and registered {RecipeLibraryEntriesAmount} base game recipe library entries",
            CookOrderData.Length);
    }
}