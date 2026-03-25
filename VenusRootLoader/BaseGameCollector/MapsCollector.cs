using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
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
                x => (RootCollector.ReadTextAssetLines($"{TextAssetPaths.DataMapEntitiesDirectory}/Names/{x}Names"),
                    RootCollector.ReadTextAssetLines($"{TextAssetPaths.DataMapEntitiesDirectory}/{x}")));

    private static readonly Dictionary<int, Dictionary<string, string[]>> MapsDialogues = new();

    private readonly ILogger<MapsCollector> _logger;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;
    private readonly IMapEntityTextAssetParser _mapEntityTextAssetParser;

    public MapsCollector(
        ILogger<MapsCollector> logger,
        ILeavesRegistry<MapLeaf> mapsRegistry,
        IMapEntityTextAssetParser mapEntityTextAssetParser)
    {
        _logger = logger;
        _mapsRegistry = mapsRegistry;
        _mapEntityTextAssetParser = mapEntityTextAssetParser;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            MapsDialogues[i] = new();
            foreach (string mapName in MapNamedIds)
            {
                string[] itemLanguageData = Resources
                    .Load<TextAsset>(
                        $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataDialoguesLocalizedMapsDirectory}/{mapName}")
                    .text
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
                _mapsRegistry.RegisterExisting(i, MapNamedIds[i], baseGameId);

            for (int j = 0; j < mapEntityData.Data.Length; j++)
            {
                string mapEntityText = mapEntityData.Data[j];
                string mapEntityName = mapEntityData.Names[j];
                MapEntity mapEntity = _mapEntityTextAssetParser.FromTextAssetSerializedString(
                    mapLeaf.InternalEntities.Count,
                    mapEntityName,
                    mapEntityText);
                mapLeaf.InternalEntities.Add(mapEntity);
            }

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
                mapLeaf.Dialogues[j] = MapsDialogues[j][MapNamedIds[i]].ToList();
        }

        _logger.LogInformation(
            "Collected and registered {MapsAmount} base game maps",
            MapNamedIds.Length);
    }
}