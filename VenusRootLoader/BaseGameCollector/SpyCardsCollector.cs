using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SpyCardsCollector : IBaseGameCollector
{
    private static readonly string[] SpyCardsData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataSpyCardsPath}").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly string SpyCardsOrderingData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataSpyCardsOrderingPath}").text
        .Trim('\n');

    private static readonly Dictionary<int, string[]> SpyCardsLanguageData = new();

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

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] spyCardLanguageData = Resources.Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataLocalizedSpyCardsPathSuffix}").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            SpyCardsLanguageData.Add(i, spyCardLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int spyCardAmount = SpyCardsData.Length;
        for (int i = 0; i < spyCardAmount; i++)
        {
            SpyCardLeaf spyCardLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            _spyCardTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataSpyCardsPath,
                SpyCardsData[i],
                spyCardLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                spyCardLeaf.LocalizedData[j] = new();
                _spyCardLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedSpyCardsPathSuffix,
                    j,
                    SpyCardsLanguageData[j][i],
                    spyCardLeaf);
            }
        }

        _spyCardOrderingTextAssetParser.FromTextAssetString(SpyCardsOrderingData, _orderedRegistry);
        _logger.LogInformation("Collected and registered {SpyCardsAmount} base game spy cards", spyCardAmount);
    }
}