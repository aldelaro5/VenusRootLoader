using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SpyCardsCollector : IBaseGameCollector
{
    private readonly string[] _spyCardsData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataSpyCardsPath);

    private readonly string _spyCardsOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataSpyCardsOrderingPath);

    private readonly Dictionary<int, string[]> _spyCardsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedSpyCardsPathSuffix);

    private readonly ILogger<SpyCardsCollector> _logger;
    private readonly IOrderedLeavesRegistry<SpyCardLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<SpyCardLeaf> _spyCardOrderingTextAssetParser;
    private readonly ITextAssetParser<SpyCardLeaf> _spyCardTextAssetParser;
    private readonly ILocalizedTextAssetParser<SpyCardLeaf> _spyCardLocalizedTextAssetParser;

    public SpyCardsCollector(
        ILogger<SpyCardsCollector> logger,
        IOrderedLeavesRegistry<SpyCardLeaf> orderedRegistry,
        IOrderingTextAssetParser<SpyCardLeaf> spyCardOrderingTextAssetParser,
        ITextAssetParser<SpyCardLeaf> spyCardTextAssetParser,
        ILocalizedTextAssetParser<SpyCardLeaf> spyCardLocalizedTextAssetParser)
    {
        _logger = logger;
        _orderedRegistry = orderedRegistry;
        _spyCardOrderingTextAssetParser = spyCardOrderingTextAssetParser;
        _spyCardTextAssetParser = spyCardTextAssetParser;
        _spyCardLocalizedTextAssetParser = spyCardLocalizedTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int spyCardAmount = _spyCardsData.Length;
        for (int i = 0; i < spyCardAmount; i++)
        {
            SpyCardLeaf spyCardLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            _spyCardTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataSpyCardsPath,
                _spyCardsData[i],
                spyCardLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                spyCardLeaf.LocalizedData[j] = new();
                _spyCardLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedSpyCardsPathSuffix,
                    j,
                    _spyCardsLanguageData[j][i],
                    spyCardLeaf);
            }
        }

        _spyCardOrderingTextAssetParser.FromTextAssetString(_spyCardsOrderingData, _orderedRegistry);
        _logger.LogInformation("Collected and registered {SpyCardsAmount} base game spy cards", spyCardAmount);
    }
}