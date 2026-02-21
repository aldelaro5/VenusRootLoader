using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameTermacadePrizesCollector : IBaseGameCollector
{
    private static readonly string[] TermacadePrizesData = Resources.Load<TextAsset>("Data/Termacade").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly ILogger<BaseGameTermacadePrizesCollector> _logger;
    private readonly ILeavesRegistry<TermacadePrizeLeaf> _termacadePrizesRegistry;
    private readonly ITextAssetParser<TermacadePrizeLeaf> _termacadePrizesTextAssetParser;

    public BaseGameTermacadePrizesCollector(
        ILeavesRegistry<TermacadePrizeLeaf> termacadePrizesRegistry,
        ILogger<BaseGameTermacadePrizesCollector> logger,
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
                "Termacade",
                termacadePrizeString,
                termacadePrizeLeaf);
        }

        _logger.LogInformation(
            "Collected and registered {TermacadePrizesAmount} base game Termacade prizes",
            TermacadePrizesData.Length);
    }
}