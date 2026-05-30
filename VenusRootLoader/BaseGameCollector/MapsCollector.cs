using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
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

    private static readonly string[] TestRoomTextData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataTestRoomMapDialoguesPath);
    
    private static readonly Dictionary<string, Dictionary<int, string[]>> MapsDialogues = new();

    private readonly ILogger<MapsCollector> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;
    private readonly IMapEntityTextAssetParser _mapEntityTextAssetParser;
    private readonly IRegistryResolver _registryResolver;

    public MapsCollector(
        ILogger<MapsCollector> logger,
        ILoggerFactory loggerFactory,
        ILeavesRegistry<MapLeaf> mapsRegistry,
        IMapEntityTextAssetParser mapEntityTextAssetParser,
        IRegistryResolver registryResolver)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _mapsRegistry = mapsRegistry;
        _mapEntityTextAssetParser = mapEntityTextAssetParser;
        _registryResolver = registryResolver;

        foreach (string mapName in MapNamedIds)
        {
            MapsDialogues[mapName] = new();
            for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
            {
                string[] itemLanguageData = Resources
                    .Load<TextAsset>(
                        $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataDialoguesLocalizedMapsDirectory}/{mapName}")
                    .text
                    .Split(StringUtils.NewlineSplitDelimiter);
                MapsDialogues[mapName].Add(i, itemLanguageData);
            }
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < MapNamedIds.Length; i++)
        {
            (string[] Names, string[] Data) mapEntityData = MapsEntityData[i];
            MapLeaf mapLeaf = _mapsRegistry.RegisterExisting(i, MapNamedIds[i], baseGameId);
            mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
                _loggerFactory.CreateLogger($"{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
                IdSequenceDirection.Increment);
            mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
                _loggerFactory.CreateLogger($"{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
                IdSequenceDirection.Increment);

            for (int j = 0; j < mapEntityData.Data.Length; j++)
            {
                string mapEntityText = mapEntityData.Data[j];
                string mapEntityName = mapEntityData.Names[j];
                _mapEntityTextAssetParser.FromTextAssetSerializedString(
                    mapLeaf,
                    baseGameId,
                    mapLeaf.EntitiesRegistry.LeavesByGameIds.Count,
                    mapEntityName,
                    mapEntityText);
            }

            if (i == 0)
            {
                for (int j = 0; j < TestRoomTextData.Length; j++)
                {
                    MapDialogueLeaf mapDialogueLeaf =
                        mapLeaf.DialoguesRegistry.RegisterExisting(j, j.ToString(), baseGameId);
                    mapDialogueLeaf.Map = mapLeaf;
                    mapDialogueLeaf.LocalizedText[0] = TestRoomTextData[j];
                }

                continue;
            }

            for (int j = 0; j < MapsDialogues[mapLeaf.NamedId].Values.Max(x => x.Length); j++)
            {
                MapDialogueLeaf mapDialogueLeaf =
                    mapLeaf.DialoguesRegistry.RegisterExisting(j, j.ToString(), baseGameId);
                mapDialogueLeaf.Map = mapLeaf;
            }

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                for (int k = 0; k < MapsDialogues[mapLeaf.NamedId][j].Length; k++)
                {
                    MapDialogueLeaf mapDialogueLeaf = mapLeaf.DialoguesRegistry.LeavesByGameIds[k];
                    mapDialogueLeaf.LocalizedText[j] = MapsDialogues[mapLeaf.NamedId][j][k];
                }
            }
        }

        // This last step is needed because while we have filled all the backing fields of the entity, the derived class
        // might need to synchronize itself with the data we just filled. This only needs to be done once per map entity
        // because we just filled them from external data, but any further modification should get synchronized immediately.
        // It also needs to be done after every map have been added so references across them works as expected.
        foreach (MapLeaf mapLeaf in _mapsRegistry.LeavesByGameIds.Values)
        {
            foreach (MapEntityLeaf mapEntity in mapLeaf.EntitiesRegistry.LeavesByGameIds.Values)
                mapEntity.InitializeFromExisting(_registryResolver);
        }

        _logger.LogInformation(
            "Collected and registered {MapsAmount} base game maps",
            MapNamedIds.Length);
    }
}