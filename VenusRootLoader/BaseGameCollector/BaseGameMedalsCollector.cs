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

    private static readonly Dictionary<int, string[]> MedalsLanguageData = new();

    private readonly string[] _badgeNamedIds = Enum.GetNames(typeof(MainManager.BadgeTypes)).ToArray();

    private readonly Sprite[] _items0Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items0");
    private readonly Sprite[] _items1Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items1");

    private readonly ILogger<BaseGameMedalsCollector> _logger;
    private readonly ITextAssetParser<MedalLeaf> _medalDataSerializer;
    private readonly ILocalizedTextAssetParser<MedalLeaf> _medalLanguageDataSerializer;
    private readonly ILeavesRegistry<MedalLeaf> _leavesRegistry;

    public BaseGameMedalsCollector(
        ILeavesRegistry<MedalLeaf> leavesRegistry,
        ILogger<BaseGameMedalsCollector> logger,
        ITextAssetParser<MedalLeaf> medalDataSerializer,
        ILocalizedTextAssetParser<MedalLeaf> medalLanguageDataSerializer)
    {
        _leavesRegistry = leavesRegistry;
        _logger = logger;
        _medalDataSerializer = medalDataSerializer;
        _medalLanguageDataSerializer = medalLanguageDataSerializer;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] itemLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/BadgeName").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            MedalsLanguageData.Add(i, itemLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _badgeNamedIds.Length; i++)
        {
            string medalNamedId = _badgeNamedIds[i];
            MedalLeaf medalLeaf = _leavesRegistry.RegisterExisting(i, medalNamedId, baseGameId);
            _medalDataSerializer.FromTextAssetSerializedString("BadgeData", MedalsData[i], medalLeaf);
            medalLeaf.WrappedSprite.Sprite = medalLeaf.Items1SpriteIndex == -1
                ? _items0Sprites[i + FirstMedalSpriteIndexInItems0]
                : _items1Sprites[medalLeaf.Items1SpriteIndex];
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                medalLeaf.LanguageData[j] = new();
                _medalLanguageDataSerializer.FromTextAssetSerializedString(
                    "BadgeName",
                    j,
                    MedalsLanguageData[j][i],
                    medalLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {MedalsAmount} base game medals", _badgeNamedIds.Length);
    }
}