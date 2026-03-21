using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class AnimIdsCollector : IBaseGameCollector
{
    private static readonly string[] AnimIdsData = Resources.Load<TextAsset>("Data/EntityValues").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly string[] _animIdNamedIds = Enum.GetNames(typeof(MainManager.AnimIDs)).ToArray();

    private readonly ILogger<AnimIdsCollector> _logger;
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsRegistry;
    private readonly ITextAssetParser<AnimIdLeaf> _animIdTextAssetParser;

    public AnimIdsCollector(
        ILogger<AnimIdsCollector> logger,
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry,
        ITextAssetParser<AnimIdLeaf> animIdTextAssetParser)
    {
        _logger = logger;
        _animIdsRegistry = animIdsRegistry;
        _animIdTextAssetParser = animIdTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _animIdNamedIds.Length; i++)
        {
            string itemNamedId = _animIdNamedIds[i];
            AnimIdLeaf animIdLeaf = _animIdsRegistry.RegisterExisting(i, itemNamedId, baseGameId);
            _animIdTextAssetParser.FromTextAssetSerializedString("EntityValues", AnimIdsData[i], animIdLeaf);
        }

        _logger.LogInformation("Collected and registered {AnimIdsAmount} base game items", _animIdNamedIds.Length);
    }
}