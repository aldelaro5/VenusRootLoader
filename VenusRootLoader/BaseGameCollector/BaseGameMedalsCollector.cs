using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameMedalsCollector : IBaseGameCollector
{
    private const int FirstMedalSpriteIndexInItems0 = 176;

    private static readonly string[] MedalsData = Resources.Load<TextAsset>("Data/BadgeData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly string MedalsOrderingData = Resources.Load<TextAsset>("Data/BadgeOrder").text
        .Trim('\n');

    private static readonly Dictionary<int, string[]> MedalsLanguageData = new();

    private readonly string[] _badgeNamedIds = Enum.GetNames(typeof(MainManager.BadgeTypes)).ToArray();

    private readonly Sprite[] _items0Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items0");
    private readonly Sprite[] _items1Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items1");

    private readonly ILogger<BaseGameMedalsCollector> _logger;
    private readonly IOrderedLeavesRegistry<MedalLeaf> _orderedRegistry;
    private readonly ITextAssetParser<MedalLeaf> _medalDataSerializer;
    private readonly IOrderingTextAssetParser<MedalLeaf> _medalOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<MedalLeaf> _medalLanguageDataSerializer;

    public BaseGameMedalsCollector(
        IOrderedLeavesRegistry<MedalLeaf> orderedRegistry,
        ILogger<BaseGameMedalsCollector> logger,
        ITextAssetParser<MedalLeaf> medalDataSerializer,
        IOrderingTextAssetParser<MedalLeaf> medalOrderingDataSerializer,
        ILocalizedTextAssetParser<MedalLeaf> medalLanguageDataSerializer)
    {
        _orderedRegistry = orderedRegistry;
        _logger = logger;
        _medalDataSerializer = medalDataSerializer;
        _medalOrderingDataSerializer = medalOrderingDataSerializer;
        _medalLanguageDataSerializer = medalLanguageDataSerializer;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] medalLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/BadgeName").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            MedalsLanguageData.Add(i, medalLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _badgeNamedIds.Length; i++)
        {
            string medalNamedId = _badgeNamedIds[i];
            MedalLeaf medalLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, medalNamedId, baseGameId);
            _medalDataSerializer.FromTextAssetSerializedString("BadgeData", MedalsData[i], medalLeaf);
            medalLeaf.WrappedSprite.Sprite = medalLeaf.Items1SpriteIndex == -1
                ? _items0Sprites[i + FirstMedalSpriteIndexInItems0]
                : _items1Sprites[medalLeaf.Items1SpriteIndex];
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                medalLeaf.LocalizedData[j] = new();
                _medalLanguageDataSerializer.FromTextAssetSerializedString(
                    "BadgeName",
                    j,
                    MedalsLanguageData[j][i],
                    medalLeaf);
            }
        }

        _medalOrderingDataSerializer.FromTextAssetString(MedalsOrderingData, _orderedRegistry);
        _logger.LogInformation("Collected and registered {MedalsAmount} base game medals", _badgeNamedIds.Length);
    }
}