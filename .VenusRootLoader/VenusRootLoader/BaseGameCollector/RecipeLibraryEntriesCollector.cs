using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RecipeLibraryEntriesCollector : IBaseGameCollector
{
    private readonly string[] _cookOrderData =
        RootCollector.ReadTextAssetLines(ResourcesPaths.DataRecipesLibraryEntriesResultItemsPath);

    private readonly string[] _cookLibraryData =
        RootCollector.ReadTextAssetLines(ResourcesPaths.DataRecipesLibraryEntriesInputItemsPath);

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

    public void CollectBaseGameData()
    {
        for (int i = 0; i < _cookOrderData.Length; i++)
        {
            string cookLibraryLine = _cookLibraryData[i];
            string cookOrderLine = _cookOrderData[i];
            RecipeLibraryEntryLeaf recipeLibraryEntryLeaf =
                _recipeLibraryEntriesRegistry.RegisterExisting(i, i.ToString());
            _recipeTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataRecipesLibraryEntriesResultItemsPath,
                cookOrderLine,
                recipeLibraryEntryLeaf);
            _recipeTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataRecipesLibraryEntriesInputItemsPath,
                cookLibraryLine,
                recipeLibraryEntryLeaf);
        }

        RootCollector.LogCollectedAmount(_logger, _recipeLibraryEntriesRegistry, _cookOrderData.Length);
    }
}