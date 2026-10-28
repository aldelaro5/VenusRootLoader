using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves.RankBonuses;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
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
            string[] split = rankBonusString.Split(StringUtils.CommaSplitDelimiter);
            int bonusType = int.Parse(split[1]);
            int gameId = _rankBonusesList.Count;

            RankBonusLeaf rankBonusLeaf = bonusType switch
            {
                0 => new SkillGrantRankBonusLeaf(gameId, Constants.BaseGameCreatorId, gameId.ToString()),
                1 => new PartyMemberStatRankBonusLeaf(gameId, Constants.BaseGameCreatorId, gameId.ToString()),
                2 => new WholePartyStatRankBonusLeaf(gameId, Constants.BaseGameCreatorId, gameId.ToString()),
                3 => new InventoryCapacityRankBonusLeaf(gameId, Constants.BaseGameCreatorId, gameId.ToString()),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<RankBonusLeaf>()
            };
            _rankBonusTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataRankBonusesPath,
                rankBonusString,
                rankBonusLeaf);
            _rankBonusesList.Add(rankBonusLeaf);
        }

        RootCollector.LogCollectedAmount<RankBonusLeaf>(_logger, _rankBonusesData.Length);
    }
}