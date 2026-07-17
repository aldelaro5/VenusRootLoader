using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using MonoMod.Utils;
using System.Text.Json;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Persistence.BudsSave;

internal sealed class BudsSaveDataDeserializer : IBudsSaveDataDeserializer
{
    private readonly ILogger<BudsSaveDataDeserializer> _logger;
    private readonly ILeavesRegistry<MedalShopLeaf> _medalShopsLeafRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesLeafRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsLeafRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry;

    public BudsSaveDataDeserializer(
        ILogger<BudsSaveDataDeserializer> logger,
        ILeavesRegistry<MedalShopLeaf> medalShopsLeafRegistry,
        ILeavesRegistry<MedalLeaf> medalsLeafRegistry,
        ILeavesRegistry<DiscoveryLeaf> discoveriesLeafRegistry,
        ILeavesRegistry<EnemyLeaf> enemiesLeafRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesLeafRegistry,
        ILeavesRegistry<RecordLeaf> recordsLeafRegistry,
        ILeavesRegistry<AreaLeaf> areasLeafRegistry,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry,
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesLeafRegistry)
    {
        _logger = logger;
        _medalShopsLeafRegistry = medalShopsLeafRegistry;
        _medalsLeafRegistry = medalsLeafRegistry;
        _discoveriesLeafRegistry = discoveriesLeafRegistry;
        _enemiesLeafRegistry = enemiesLeafRegistry;
        _recipeLibraryEntriesLeafRegistry = recipeLibraryEntriesLeafRegistry;
        _recordsLeafRegistry = recordsLeafRegistry;
        _areasLeafRegistry = areasLeafRegistry;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagstringsLeafRegistry = flagstringsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
        _crystalBerriesLeafRegistry = crystalBerriesLeafRegistry;
    }

    public void DeserializeBudsSaveData(Dictionary<string, string> budsSaveDataByIds, StagingLoadData stagingLoadData)
    {
        BudSaveData allBudsSaveData = RemapAndMergeAllBudsSaveData(budsSaveDataByIds);

        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.MedalShops,
            _medalShopsLeafRegistry,
            nameof(MedalShopLeaf),
            stagingLoadData,
            (loadData, leaf) =>
            {
                List<int> startingStock = leaf.StartingMedalsStock.Select(m => m.GameId).ToList();
                loadData.AvaliableBadgePool.Add(startingStock);
                loadData.BadgeShops.Add(startingStock);
            },
            (loadData, data, leaf) => LoadMedalShop(data, loadData, leaf));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.DiscoveryUnlocks,
            _discoveriesLeafRegistry,
            nameof(DiscoveryLeaf),
            stagingLoadData,
            (loadData, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Discoveries].Add(false),
            (loadData, data, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Discoveries].Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.Enemies,
            _enemiesLeafRegistry,
            nameof(EnemyLeaf),
            stagingLoadData,
            (loadData, _) =>
            {
                loadData.LibraryStuff[(int)MainManager.LibraryPages.Bestiary].Add(false);
                loadData.EnemyEncounter.Add([0, 0]);
            },
            (loadData, data, _) =>
            {
                loadData.LibraryStuff[(int)MainManager.LibraryPages.Bestiary].Add(data.IsBestiaryEntryUnlocked);
                loadData.EnemyEncounter.Add([data.AmountSeen, data.AmountDefeated]);
            });
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.RecipeLibraryEntryUnlocks,
            _recipeLibraryEntriesLeafRegistry,
            nameof(RecipeLibraryEntryLeaf),
            stagingLoadData,
            (loadData, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Recipes].Add(false),
            (loadData, data, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Recipes].Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.RecordUnlocks,
            _recordsLeafRegistry,
            nameof(RecordLeaf),
            stagingLoadData,
            (loadData, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Logbook].Add(false),
            (loadData, data, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Logbook].Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.AreaUnlocks,
            _areasLeafRegistry,
            nameof(AreaLeaf),
            stagingLoadData,
            (loadData, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Map].Add(false),
            (loadData, data, _) => loadData.LibraryStuff[(int)MainManager.LibraryPages.Map].Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.Flags,
            _flagsLeafRegistry,
            nameof(FlagLeaf),
            stagingLoadData,
            (loadData, _) => loadData.Flags.Add(false),
            (loadData, data, _) => loadData.Flags.Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.Flagstrings,
            _flagstringsLeafRegistry,
            nameof(FlagstringLeaf),
            stagingLoadData,
            (loadData, _) => loadData.Flagstrings.Add(""),
            (loadData, data, _) => loadData.Flagstrings.Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.Flagvars,
            _flagvarsLeafRegistry,
            nameof(FlagvarLeaf),
            stagingLoadData,
            (loadData, _) => loadData.Flagvars.Add(0),
            (loadData, data, _) => loadData.Flagvars.Add(data));
        LoadDictionarySaveDataMatchingLeaves(
            allBudsSaveData.CrystalBerries,
            _crystalBerriesLeafRegistry,
            nameof(CrystalBerryLeaf),
            stagingLoadData,
            (loadData, _) => loadData.CrystalBerryFlags.Add(false),
            (loadData, data, _) => loadData.CrystalBerryFlags.Add(data));
    }

    private static BudSaveData RemapAndMergeAllBudsSaveData(Dictionary<string, string> budsSaveDataByIds)
    {
        BudSaveData allBudsSaveData = new()
        {
            MedalShops = new(),
            DiscoveryUnlocks = new(),
            Enemies = new(),
            RecipeLibraryEntryUnlocks = new(),
            RecordUnlocks = new(),
            AreaUnlocks = new(),
            Flags = new(),
            Flagstrings = new(),
            Flagvars = new(),
            CrystalBerries = new()
        };

        foreach (KeyValuePair<string, string> budSave in budsSaveDataByIds)
        {
            BudSaveData? budSaveData = JsonSerializer.Deserialize<BudSaveData>(budSave.Value);
            if (budSaveData == null)
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"The bud save data of the bud {budSave.Key} deserialized to null");
            }

            allBudsSaveData.MedalShops.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.MedalShops));
            allBudsSaveData.DiscoveryUnlocks.AddRange(
                RemapSaveDataDictionary(budSave.Key, budSaveData.DiscoveryUnlocks));
            allBudsSaveData.Enemies.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.Enemies));
            allBudsSaveData.RecipeLibraryEntryUnlocks.AddRange(
                RemapSaveDataDictionary(budSave.Key, budSaveData.RecipeLibraryEntryUnlocks));
            allBudsSaveData.RecordUnlocks.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.RecordUnlocks));
            allBudsSaveData.AreaUnlocks.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.AreaUnlocks));
            allBudsSaveData.Flags.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.Flags));
            allBudsSaveData.Flagstrings.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.Flagstrings));
            allBudsSaveData.Flagvars.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.Flagvars));
            allBudsSaveData.CrystalBerries.AddRange(RemapSaveDataDictionary(budSave.Key, budSaveData.CrystalBerries));
        }

        return allBudsSaveData;
    }

    private static Dictionary<string, T> RemapSaveDataDictionary<T>(
        string creatorId,
        Dictionary<string, T> dataByNamedId)
    {
        return dataByNamedId.ToDictionary(x => $"{creatorId}{Constants.LeafEffectiveIdSeparator}{x.Key}", x => x.Value);
    }

    private void LoadMedalShop(MedalShopLeafSaveData data, StagingLoadData loadData, MedalShopLeaf leaf)
    {
        List<int> availableBadgePoolGameIds = new();
        foreach (string medalEffectiveId in data.AvailablePool)
        {
            if (!_medalsLeafRegistry.LeavesByEffectiveIds.TryGetValue(medalEffectiveId, out MedalLeaf medalLeaf))
            {
                (string CreatorId, string NamedId) idParts = EffectiveLeafId.SplitParts(medalEffectiveId);
                _logger.LogWarning(
                    "The MedalShopLeaf named {medalShopNamedId} has a MedalLeaf named {medalNamedId} created by {medalCreatorId} " +
                    "in its available pool while no such MedalLeaf exists in the registry. " +
                    "It will be skipped, but the save will still be loaded.",
                    leaf.NamedId,
                    idParts.NamedId,
                    idParts.CreatorId);
                continue;
            }

            availableBadgePoolGameIds.Add(medalLeaf.GameId);
        }

        List<int> shopStockGameIds = new();
        foreach (string medalEffectiveId in data.ShopStock)
        {
            if (!_medalsLeafRegistry.LeavesByEffectiveIds.TryGetValue(medalEffectiveId, out MedalLeaf medalLeaf))
            {
                (string CreatorId, string NamedId) idParts = EffectiveLeafId.SplitParts(medalEffectiveId);
                _logger.LogWarning(
                    "The MedalShopLeaf named {medalShopNamedId} has a MedalLeaf named {medalNamedId} created by {medalCreatorId} " +
                    "in its shop stock while no such MedalLeaf exists in the registry. " +
                    "It will be skipped, but the save will still be loaded.",
                    leaf.NamedId,
                    idParts.NamedId,
                    idParts.CreatorId);
                continue;
            }

            shopStockGameIds.Add(medalLeaf.GameId);
        }

        loadData.AvaliableBadgePool.Add(availableBadgePoolGameIds);
        loadData.BadgeShops.Add(shopStockGameIds);
    }

    private void LoadDictionarySaveDataMatchingLeaves<TSaveData, TLeaf>(
        Dictionary<string, TSaveData> dataByNamedIds,
        ILeavesRegistry<TLeaf> leavesRegistry,
        string leafTypeName,
        StagingLoadData stagingLoadData,
        Action<StagingLoadData, TLeaf> loadDefaultAction,
        Action<StagingLoadData, TSaveData, TLeaf> loadSaveData)
        where TLeaf : Leaf
    {
        Dictionary<int, TSaveData> dataByGameIds = new();
        foreach (KeyValuePair<string, TSaveData> data in dataByNamedIds)
        {
            if (!leavesRegistry.LeavesByEffectiveIds.TryGetValue(
                    data.Key,
                    out TLeaf leaf))
            {
                (string CreatorId, string NamedId) idParts = EffectiveLeafId.SplitParts(data.Key);
                _logger.LogWarning(
                    "The {leafTypeName} named {namedId} created by {creatorId} does not exists in the registry. " +
                    "It will be skipped, but the save will still be loaded.",
                    leafTypeName,
                    idParts.NamedId,
                    idParts.CreatorId);
                continue;
            }

            dataByGameIds.Add(leaf.GameId, data.Value);
        }

        int baseGameAmount = leavesRegistry.LeavesByEffectiveIds.Values
            .Count(x => x.CreatorId == Constants.BaseGameCreatorId);
        for (int i = baseGameAmount; i < leavesRegistry.LeavesByGameIds.Values.Count; i++)
        {
            TLeaf leaf = leavesRegistry.LeavesByGameIds[i];
            if (!dataByGameIds.TryGetValue(i, out TSaveData data))
            {
                _logger.LogWarning(
                    "The {leafTypeName} named {namedId} created by {creatorId} wasn't found in any of the buds save data. " +
                    "It will be loaded with default data.",
                    leafTypeName,
                    leaf.NamedId,
                    leaf.CreatorId);

                loadDefaultAction(stagingLoadData, leaf);
                continue;
            }

            loadSaveData(stagingLoadData, data, leaf);
        }
    }
}