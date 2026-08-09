using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RankBonusesCollector : IBaseGameCollector
{
    private readonly string[] _rankBonusesData =
        RootCollector.ReadTextAssetLines(ResourcesPaths.DataRankBonusesPath);

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

    public void CollectBaseGameData()
    {
        for (int i = 0; i < _rankBonusesData.Length; i++)
        {
            string rankBonusString = _rankBonusesData[i];
            RankBonusLeaf rankBonusLeaf =
                _rankBonusesRegistry.RegisterExisting(i, i.ToString());
            _rankBonusTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataRankBonusesPath,
                rankBonusString,
                rankBonusLeaf);
        }

        RootCollector.LogCollectedAmount(_logger, _rankBonusesRegistry, _rankBonusesData.Length);
    }
}