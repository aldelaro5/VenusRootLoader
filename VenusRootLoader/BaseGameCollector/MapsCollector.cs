using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MapsCollector : IBaseGameCollector
{
    private static readonly string[] MapNamedIds = Enum.GetNames(typeof(MainManager.Maps)).ToArray();

    private static readonly Dictionary<int, (string[] Names, string[] Data)> MapsEntityData =
        Enumerable.Range(0, MapNamedIds.Length)
            .ToDictionary(
                x => x,
                x => (Resources.Load<TextAsset>($"Data/EntityData/Names/{x}Names").text.Trim('\n')
                        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries),
                    Resources.Load<TextAsset>($"Data/EntityData/{x}").text.Trim('\n')
                        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries)));

    private static readonly Dictionary<int, Dictionary<string, string[]>> MapsDialogues = new();

    private readonly ILogger<MapsCollector> _logger;
    private readonly ILeavesRegistry<MapLeaf> _rankBonusesRegistry;
    private readonly ITextAssetParser<MapLeaf.MapEntity> _rankBonusTextAssetParser;

    public MapsCollector(
        ILogger<MapsCollector> logger,
        ILeavesRegistry<MapLeaf> rankBonusesRegistry,
        ITextAssetParser<MapLeaf.MapEntity> rankBonusTextAssetParser)
    {
        _logger = logger;
        _rankBonusesRegistry = rankBonusesRegistry;
        _rankBonusTextAssetParser = rankBonusTextAssetParser;


        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            MapsDialogues[i] = new();
            foreach (string mapName in MapNamedIds)
            {
                string[] itemLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/Maps/{mapName}").text
                    .Split(StringUtils.NewlineSplitDelimiter);
                MapsDialogues[i].Add(mapName, itemLanguageData);
            }
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < MapNamedIds.Length; i++)
        {
            (string[] Names, string[] Data) mapEntityData = MapsEntityData[i];
            MapLeaf mapLeaf =
                _rankBonusesRegistry.RegisterExisting(i, MapNamedIds[i], baseGameId);

            for (int j = 0; j < mapEntityData.Data.Length; j++)
            {
                string mapEntityText = mapEntityData.Data[j];
                string mapEntityName = mapEntityData.Names[j];
                MapLeaf.MapEntity mapEntity = mapLeaf.Entities.CreateNew();
                _rankBonusTextAssetParser.FromTextAssetSerializedString(
                    $"EntityData/Names/{i}Names",
                    mapEntityName,
                    mapEntity);
                _rankBonusTextAssetParser.FromTextAssetSerializedString(
                    $"EntityData/{i}",
                    mapEntityText,
                    mapEntity);
            }

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
                mapLeaf.Dialogues[j] = MapsDialogues[j][MapNamedIds[i]].ToList();
        }

        _logger.LogInformation(
            "Collected and registered {MapsAmount} base game maps",
            MapNamedIds.Length);
    }
}