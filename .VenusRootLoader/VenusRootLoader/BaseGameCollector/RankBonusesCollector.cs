using Microsoft.Extensions.Logging;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves.RankBonuses;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RankBonusesCollector : IBaseGameCollector
{
    private readonly string[] _rankBonusesData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataRankBonusesPath);

    private readonly ILogger<RankBonusesCollector> _logger;
    private readonly ILeafList<RankBonusLeaf> _rankBonusesList;
    private readonly ITextAssetParser<RankBonusLeaf> _rankBonusTextAssetParser;

    public RankBonusesCollector(
        ILogger<RankBonusesCollector> logger,
        ILeafList<RankBonusLeaf> rankBonusesList,
        ITextAssetParser<RankBonusLeaf> rankBonusTextAssetParser)
    {
        _logger = logger;
        _rankBonusesList = rankBonusesList;
        _rankBonusTextAssetParser = rankBonusTextAssetParser;
    }

    public void CollectBaseGameData()
    {
        foreach (string rankBonusString in _rankBonusesData)
        {
            int gameId = _rankBonusesList.Count;
            RankBonusLeaf rankBonusLeaf = new(gameId, Constants.BaseGameCreatorId, gameId.ToString());
            _rankBonusTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataRankBonusesPath,
                rankBonusString,
                rankBonusLeaf);
            _rankBonusesList.Add(rankBonusLeaf);
        }

        RootCollector.LogCollectedAmount<RankBonusLeaf>(_logger, _rankBonusesData.Length);
    }
}