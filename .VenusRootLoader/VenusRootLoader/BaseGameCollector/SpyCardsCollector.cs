using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SpyCardsCollector : IBaseGameCollector
{
    private readonly string[] _spyCardsData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataSpyCardsPath);

    private readonly string _spyCardsOrderingData =
        RootCollector.ReadWholeTextAsset(ResourcesPaths.DataSpyCardsOrderingPath);

    private readonly Dictionary<int, string[]> _spyCardsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedSpyCardsPathSuffix);

    private readonly ILogger<SpyCardsCollector> _logger;
    private readonly IOrderedLeavesRegistry<SpyCardLeaf> _orderedRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly IOrderingTextAssetParser<SpyCardLeaf> _spyCardOrderingTextAssetParser;
    private readonly ITextAssetParser<SpyCardLeaf> _spyCardTextAssetParser;
    private readonly ILocalizedTextAssetParser<SpyCardLeaf> _spyCardLocalizedTextAssetParser;

    public SpyCardsCollector(
        ILogger<SpyCardsCollector> logger,
        IOrderedLeavesRegistry<SpyCardLeaf> orderedRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        IOrderingTextAssetParser<SpyCardLeaf> spyCardOrderingTextAssetParser,
        ITextAssetParser<SpyCardLeaf> spyCardTextAssetParser,
        ILocalizedTextAssetParser<SpyCardLeaf> spyCardLocalizedTextAssetParser)
    {
        _logger = logger;
        _orderedRegistry = orderedRegistry;
        _spyCardOrderingTextAssetParser = spyCardOrderingTextAssetParser;
        _spyCardTextAssetParser = spyCardTextAssetParser;
        _spyCardLocalizedTextAssetParser = spyCardLocalizedTextAssetParser;
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        int spyCardAmount = _spyCardsData.Length;
        for (int i = 0; i < spyCardAmount; i++)
        {
            SpyCardLeaf spyCardLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString());
            _spyCardTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataSpyCardsPath,
                _spyCardsData[i],
                spyCardLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                spyCardLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _spyCardLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedSpyCardsPathSuffix,
                    j,
                    _spyCardsLanguageData[j][i],
                    spyCardLeaf);
            }
        }

        _spyCardOrderingTextAssetParser.FromTextAssetString(_spyCardsOrderingData, _orderedRegistry);
        RootCollector.LogCollectedAmount(_logger, _languageRegistry, spyCardAmount);
    }
}