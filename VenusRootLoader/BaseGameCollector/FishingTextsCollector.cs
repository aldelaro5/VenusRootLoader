using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class FishingTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> FishingTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedFishingTextsPathSuffix);

    private readonly ILogger<FishingTextsCollector> _logger;
    private readonly ILeavesRegistry<FishingTextLeaf> _fishingTextsRegistry;
    private readonly ILocalizedTextAssetParser<FishingTextLeaf> _fishingTextLocalizedTextAssetParser;

    public FishingTextsCollector(
        ILogger<FishingTextsCollector> logger,
        ILocalizedTextAssetParser<FishingTextLeaf> fishingTextLocalizedTextAssetParser,
        ILeavesRegistry<FishingTextLeaf> fishingTextsRegistry)
    {
        _logger = logger;
        _fishingTextLocalizedTextAssetParser = fishingTextLocalizedTextAssetParser;
        _fishingTextsRegistry = fishingTextsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int fishingTextsAmount = FishingTextsLanguageData.Values.First().Length;
        for (int i = 0; i < fishingTextsAmount; i++)
        {
            FishingTextLeaf fishingTextLeaf = _fishingTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _fishingTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedFishingTextsPathSuffix,
                    j,
                    FishingTextsLanguageData[j][i],
                    fishingTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {FishingTextsAmount} base game fishing texts",
            fishingTextsAmount);
    }
}