using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class TermacadePrizesCollector : IBaseGameCollector
{
    private static readonly string[] TermacadePrizesData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataTermacadePrizesPath}").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly ILogger<TermacadePrizesCollector> _logger;
    private readonly ILeavesRegistry<TermacadePrizeLeaf> _termacadePrizesRegistry;
    private readonly ITextAssetParser<TermacadePrizeLeaf> _termacadePrizesTextAssetParser;

    public TermacadePrizesCollector(
        ILeavesRegistry<TermacadePrizeLeaf> termacadePrizesRegistry,
        ILogger<TermacadePrizesCollector> logger,
        ITextAssetParser<TermacadePrizeLeaf> termacadePrizesTextAssetParser)
    {
        _termacadePrizesRegistry = termacadePrizesRegistry;
        _logger = logger;
        _termacadePrizesTextAssetParser = termacadePrizesTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < TermacadePrizesData.Length; i++)
        {
            string termacadePrizeString = TermacadePrizesData[i];
            TermacadePrizeLeaf termacadePrizeLeaf =
                _termacadePrizesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _termacadePrizesTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataTermacadePrizesPath,
                termacadePrizeString,
                termacadePrizeLeaf);
        }

        _logger.LogInformation(
            "Collected and registered {TermacadePrizesAmount} base game Termacade prizes",
            TermacadePrizesData.Length);
    }
}