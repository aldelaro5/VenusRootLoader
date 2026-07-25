using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MedalsCollector : IBaseGameCollector
{
    // Items0 contains both items and medals sprites, but they're both contiguous in the array and the game hardcodes the
    // index that separates these 2 regions so we need to hardcode this too.
    private const int FirstMedalSpriteIndexInItems0 = 176;

    private readonly string[] _medalsData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataMedalsPath);

    private readonly string _medalsOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataMedalsOrderingPath);

    private readonly Dictionary<int, string[]> _medalsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedMedalPathSuffix);

    private readonly string[] _badgeNamedIds = Enum.GetNames(typeof(MainManager.BadgeTypes)).ToArray();

    private readonly Sprite[] _items0Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems0Path}");

    private readonly Sprite[] _items1Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems1Path}");

    private readonly ILogger<MedalsCollector> _logger;
    private readonly IOrderedLeavesRegistry<MedalLeaf> _orderedRegistry;
    private readonly ITextAssetParser<MedalLeaf> _medalDataSerializer;
    private readonly IOrderingTextAssetParser<MedalLeaf> _medalOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<MedalLeaf> _medalLanguageDataSerializer;

    public MedalsCollector(
        IOrderedLeavesRegistry<MedalLeaf> orderedRegistry,
        ILogger<MedalsCollector> logger,
        ITextAssetParser<MedalLeaf> medalDataSerializer,
        IOrderingTextAssetParser<MedalLeaf> medalOrderingDataSerializer,
        ILocalizedTextAssetParser<MedalLeaf> medalLanguageDataSerializer)
    {
        _orderedRegistry = orderedRegistry;
        _logger = logger;
        _medalDataSerializer = medalDataSerializer;
        _medalOrderingDataSerializer = medalOrderingDataSerializer;
        _medalLanguageDataSerializer = medalLanguageDataSerializer;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _badgeNamedIds.Length; i++)
        {
            string medalNamedId = _badgeNamedIds[i];
            MedalLeaf medalLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, medalNamedId, baseGameId);
            _medalDataSerializer.FromTextAssetSerializedString(
                TextAssetPaths.DataMedalsPath,
                _medalsData[i],
                medalLeaf);
            medalLeaf.WrappedSprite.Sprite = medalLeaf.Items1SpriteIndex == -1
                ? _items0Sprites[i + FirstMedalSpriteIndexInItems0]
                : _items1Sprites[medalLeaf.Items1SpriteIndex];
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                medalLeaf.LocalizedData[j] = new();
                _medalLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedMedalPathSuffix,
                    j,
                    _medalsLanguageData[j][i],
                    medalLeaf);
            }
        }

        _medalOrderingDataSerializer.FromTextAssetString(_medalsOrderingData, _orderedRegistry);
        _logger.LogInformation("Collected and registered {MedalsAmount} base game medals", _badgeNamedIds.Length);
    }
}