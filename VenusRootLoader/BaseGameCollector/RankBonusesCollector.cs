using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RankBonusesCollector : IBaseGameCollector
{
    private static readonly string[] RankBonusesData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataRankBonusesPath);

    private readonly ILogger<RankBonusesCollector> _logger;
    private readonly ILeavesRegistry<RankBonusLeaf> _rankBonusesRegistry;
    private readonly ITextAssetParser<RankBonusLeaf> _rankBonusTextAssetParser;

    public RankBonusesCollector(
        ILogger<RankBonusesCollector> logger,
        ILeavesRegistry<RankBonusLeaf> rankBonusesRegistry,
        ITextAssetParser<RankBonusLeaf> rankBonusTextAssetParser)
    {
        _logger = logger;
        _rankBonusesRegistry = rankBonusesRegistry;
        _rankBonusTextAssetParser = rankBonusTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < RankBonusesData.Length; i++)
        {
            string rankBonusString = RankBonusesData[i];
            RankBonusLeaf rankBonusLeaf =
                _rankBonusesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _rankBonusTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataRankBonusesPath,
                rankBonusString,
                rankBonusLeaf);
        }

        _logger.LogInformation(
            "Collected and registered {RankBonusesAmount} base game rank bonuses",
            RankBonusesData.Length);
    }
}