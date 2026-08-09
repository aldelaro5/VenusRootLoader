using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MedalsCollector : IBaseGameCollector
{
    // Items0 contains both items and medals sprites, but they're both contiguous in the array and the game hardcodes the
    // index that separates these 2 regions so we need to hardcode this too.
    private const int FirstMedalSpriteIndexInItems0 = 176;

    private const string SpritesItemsItems0ResourcesPath =
        $"{ResourcesPaths.RootSpritesPathPrefix}{ResourcesPaths.SpritesItems0Path}";

    private const string SpritesItemsItems1ResourcesPath =
        $"{ResourcesPaths.RootSpritesPathPrefix}{ResourcesPaths.SpritesItems1Path}";

    private readonly string[] _medalsData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataMedalsPath);

    private readonly string _medalsOrderingData =
        RootCollector.ReadWholeTextAsset(ResourcesPaths.DataMedalsOrderingPath);

    private readonly Dictionary<int, string[]> _medalsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedMedalPathSuffix);

    private readonly string[] _badgeNamedIds = Enum.GetNames(typeof(MainManager.BadgeTypes)).ToArray();

    private readonly ILogger<MedalsCollector> _logger;
    private readonly IOrderedLeavesRegistry<MedalLeaf> _orderedRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly ITextAssetParser<MedalLeaf> _medalDataSerializer;
    private readonly IOrderingTextAssetParser<MedalLeaf> _medalOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<MedalLeaf> _medalLanguageDataSerializer;

    public MedalsCollector(
        IOrderedLeavesRegistry<MedalLeaf> orderedRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
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
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        for (int i = 0; i < _badgeNamedIds.Length; i++)
        {
            string medalNamedId = _badgeNamedIds[i];
            MedalLeaf medalLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, medalNamedId);
            _medalDataSerializer.FromTextAssetSerializedString(
                ResourcesPaths.DataMedalsPath,
                _medalsData[i],
                medalLeaf);
            medalLeaf.Sprite = medalLeaf.Items1SpriteIndex == -1
                ? new AssetLoaderFromResources<Sprite>(
                    SpritesItemsItems0ResourcesPath,
                    i + FirstMedalSpriteIndexInItems0)
                : new AssetLoaderFromResources<Sprite>(SpritesItemsItems1ResourcesPath, medalLeaf.Items1SpriteIndex);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                medalLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _medalLanguageDataSerializer.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedMedalPathSuffix,
                    j,
                    _medalsLanguageData[j][i],
                    medalLeaf);
            }
        }

        _medalOrderingDataSerializer.FromTextAssetString(_medalsOrderingData, _orderedRegistry);
        RootCollector.LogCollectedAmount(_logger, _orderedRegistry.Registry, _badgeNamedIds.Length);
    }
}