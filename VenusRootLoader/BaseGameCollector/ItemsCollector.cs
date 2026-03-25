using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ItemsCollector : IBaseGameCollector
{
    private const int ItemsSpritesAmountInItems0 = 176;

    private static readonly string[] ItemsData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataItemsPath}").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly Dictionary<int, string[]> ItemsLanguageData = new();

    private readonly string[] _itemNamedIds = Enum.GetNames(typeof(MainManager.Items))
        .TakeWhile(v => v != nameof(MainManager.Items.None))
        .ToArray();

    private readonly Sprite[] _items0Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems0Path}");

    private readonly Sprite[] _items1Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems1Path}");

    private readonly ILogger<ItemsCollector> _logger;
    private readonly ITextAssetParser<ItemLeaf> _itemDataSerializer;
    private readonly ILocalizedTextAssetParser<ItemLeaf> _itemLanguageDataSerializer;
    private readonly ILeavesRegistry<ItemLeaf> _leavesRegistry;

    public ItemsCollector(
        ILeavesRegistry<ItemLeaf> leavesRegistry,
        ILogger<ItemsCollector> logger,
        ITextAssetParser<ItemLeaf> itemDataSerializer,
        ILocalizedTextAssetParser<ItemLeaf> itemLanguageDataSerializer)
    {
        _leavesRegistry = leavesRegistry;
        _logger = logger;
        _itemDataSerializer = itemDataSerializer;
        _itemLanguageDataSerializer = itemLanguageDataSerializer;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] itemLanguageData = Resources.Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataLocalizedItemsPathSuffix}").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            // Workaround a game bug where not all languages has the last line about BigBerry
            if (itemLanguageData.Length != _itemNamedIds.Length)
                itemLanguageData = itemLanguageData.Append("RESERVED@Desc@Desc@a").ToArray();
            ItemsLanguageData.Add(i, itemLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _itemNamedIds.Length; i++)
        {
            string itemNamedId = _itemNamedIds[i];
            ItemLeaf itemLeaf = _leavesRegistry.RegisterExisting(i, itemNamedId, baseGameId);
            _itemDataSerializer.FromTextAssetSerializedString(TextAssetPaths.DataItemsPath, ItemsData[i], itemLeaf);
            itemLeaf.WrappedSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                itemLeaf.LocalizedData[j] = new();
                _itemLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedItemsPathSuffix,
                    j,
                    ItemsLanguageData[j][i],
                    itemLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {ItemsAmount} base game items", _itemNamedIds.Length);
    }
}