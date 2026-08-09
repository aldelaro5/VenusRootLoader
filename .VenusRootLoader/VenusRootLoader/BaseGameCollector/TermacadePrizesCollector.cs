using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class TermacadePrizesCollector : IBaseGameCollector
{
    private readonly string[] _termacadePrizesData =
        RootCollector.ReadTextAssetLines(ResourcesPaths.DataTermacadePrizesPath);

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

    public void CollectBaseGameData()
    {
        for (int i = 0; i < _termacadePrizesData.Length; i++)
        {
            string termacadePrizeString = _termacadePrizesData[i];
            TermacadePrizeLeaf termacadePrizeLeaf =
                _termacadePrizesRegistry.RegisterExisting(i, i.ToString());
            _termacadePrizesTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataTermacadePrizesPath,
                termacadePrizeString,
                termacadePrizeLeaf);
        }

        RootCollector.LogCollectedAmount(_logger, _termacadePrizesRegistry, _termacadePrizesData.Length);
    }
}