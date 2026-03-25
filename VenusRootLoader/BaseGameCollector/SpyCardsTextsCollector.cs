using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SpyCardsTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> SpyCardsTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedSpyCardsTextsPathSuffix);

    private readonly ILogger<SpyCardsTextsCollector> _logger;
    private readonly ILeavesRegistry<SpyCardsTextLeaf> _spyCardsTextsRegistry;
    private readonly ILocalizedTextAssetParser<SpyCardsTextLeaf> _spyCardsTextLocalizedTextAssetParser;

    public SpyCardsTextsCollector(
        ILogger<SpyCardsTextsCollector> logger,
        ILocalizedTextAssetParser<SpyCardsTextLeaf> spyCardsTextLocalizedTextAssetParser,
        ILeavesRegistry<SpyCardsTextLeaf> spyCardsTextsRegistry)
    {
        _logger = logger;
        _spyCardsTextLocalizedTextAssetParser = spyCardsTextLocalizedTextAssetParser;
        _spyCardsTextsRegistry = spyCardsTextsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int spyCardsTextsAmount = SpyCardsTextsLanguageData.Values.First().Length;
        for (int i = 0; i < spyCardsTextsAmount; i++)
        {
            SpyCardsTextLeaf spyCardsTextLeaf = _spyCardsTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _spyCardsTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedSpyCardsTextsPathSuffix,
                    j,
                    SpyCardsTextsLanguageData[j][i],
                    spyCardsTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {SpyCardsTextsAmount} base game Spy Cards texts",
            spyCardsTextsAmount);
    }
}