using System.Text.Json;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Persistence.BudsSave;

internal sealed class BudsSaveDataSerializer : IBudsSaveDataSerializer
{
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly IGameDataRuntimeState _gameDataRuntimeState;
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

    public BudsSaveDataSerializer(
        IGameDataRuntimeState gameDataRuntimeState,
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
        _gameDataRuntimeState = gameDataRuntimeState;
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

    public Dictionary<string, string> GetBudsSaveDataFromRuntimeState()
    {
        Dictionary<string, string> budsSaveData = new();

        Dictionary<string, List<MedalShopLeaf>> medalShopsByCreatorId = GetLeavesByCreatorIds(_medalShopsLeafRegistry);
        Dictionary<string, List<DiscoveryLeaf>>
            discoveriesByCreatorId = GetLeavesByCreatorIds(_discoveriesLeafRegistry);
        Dictionary<string, List<EnemyLeaf>> enemiesByCreatorId = GetLeavesByCreatorIds(_enemiesLeafRegistry);
        Dictionary<string, List<RecipeLibraryEntryLeaf>> recipeLibraryEntriesByCreatorId =
            GetLeavesByCreatorIds(_recipeLibraryEntriesLeafRegistry);
        Dictionary<string, List<RecordLeaf>> recordsByCreatorId = GetLeavesByCreatorIds(_recordsLeafRegistry);
        Dictionary<string, List<AreaLeaf>> areasByCreatorId = GetLeavesByCreatorIds(_areasLeafRegistry);
        Dictionary<string, List<FlagLeaf>> flagsByCreatorId = GetLeavesByCreatorIds(_flagsLeafRegistry);
        Dictionary<string, List<FlagstringLeaf>> flagstringsByCreatorId =
            GetLeavesByCreatorIds(_flagstringsLeafRegistry);
        Dictionary<string, List<FlagvarLeaf>> flagvarsByCreatorId = GetLeavesByCreatorIds(_flagvarsLeafRegistry);
        Dictionary<string, List<CrystalBerryLeaf>> crystalBerriesByCreatorId =
            GetLeavesByCreatorIds(_crystalBerriesLeafRegistry);

        HashSet<string> allBudsWithSaveData =
        [
            .. medalShopsByCreatorId.Keys,
            .. discoveriesByCreatorId.Keys,
            .. enemiesByCreatorId.Keys,
            .. recipeLibraryEntriesByCreatorId.Keys,
            .. recordsByCreatorId.Keys,
            .. areasByCreatorId.Keys,
            .. flagsByCreatorId.Keys,
            .. flagstringsByCreatorId.Keys,
            .. flagvarsByCreatorId.Keys,
            .. crystalBerriesByCreatorId.Keys
        ];

        foreach (string budId in allBudsWithSaveData)
        {
            BudSaveData budSaveData = GetBudSaveDataFromRuntimeState(
                GetNormalizedLeavesList(budId, medalShopsByCreatorId),
                GetNormalizedLeavesList(budId, discoveriesByCreatorId),
                GetNormalizedLeavesList(budId, enemiesByCreatorId),
                GetNormalizedLeavesList(budId, recipeLibraryEntriesByCreatorId),
                GetNormalizedLeavesList(budId, recordsByCreatorId),
                GetNormalizedLeavesList(budId, areasByCreatorId),
                GetNormalizedLeavesList(budId, flagsByCreatorId),
                GetNormalizedLeavesList(budId, flagstringsByCreatorId),
                GetNormalizedLeavesList(budId, flagvarsByCreatorId),
                GetNormalizedLeavesList(budId, crystalBerriesByCreatorId));
            string jsonBudSaveData = JsonSerializer.Serialize(budSaveData, _serializerOptions);
            budsSaveData.Add(budId, jsonBudSaveData);
        }

        return budsSaveData;
    }

    private BudSaveData GetBudSaveDataFromRuntimeState(
        List<MedalShopLeaf> medalShopLeaves,
        List<DiscoveryLeaf> discoveryLeaves,
        List<EnemyLeaf> enemyLeaves,
        List<RecipeLibraryEntryLeaf> recipeLibraryEntryLeaves,
        List<RecordLeaf> recordLeaves,
        List<AreaLeaf> areaLeaves,
        List<FlagLeaf> flagLeaves,
        List<FlagstringLeaf> flagstringLeaves,
        List<FlagvarLeaf> flagvarLeaves,
        List<CrystalBerryLeaf> crystalBerryLeaves)
    {
        return new()
        {
            MedalShops = medalShopLeaves.ToDictionary(
                medalShopLeaf => medalShopLeaf.NamedId,
                medalShopLeaf => new MedalShopLeafSaveData
                {
                    AvailablePool = _gameDataRuntimeState.AvailableBadgePool[medalShopLeaf.GameId]
                        .Select(medalGameId => _medalsLeafRegistry.LeavesByGameIds[medalGameId].EffectiveId)
                        .ToList(),
                    ShopStock = _gameDataRuntimeState.BadgeShops[medalShopLeaf.GameId]
                        .Select(medalGameId => _medalsLeafRegistry.LeavesByGameIds[medalGameId].EffectiveId)
                        .ToList()
                }),
            DiscoveryUnlocks = discoveryLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.LibraryStuff[(int)MainManager.LibraryPages.Discoveries, x.GameId]),
            Enemies = enemyLeaves.ToDictionary(
                x => x.NamedId,
                x => new EnemySaveData
                {
                    IsBestiaryEntryUnlocked =
                        _gameDataRuntimeState.LibraryStuff[(int)MainManager.LibraryPages.Bestiary, x.GameId],
                    AmountSeen = _gameDataRuntimeState.EnemyEncounter[x.GameId, 0],
                    AmountDefeated = _gameDataRuntimeState.EnemyEncounter[x.GameId, 1]
                }),
            RecipeLibraryEntryUnlocks = recipeLibraryEntryLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.LibraryStuff[(int)MainManager.LibraryPages.Recipes, x.GameId]),
            RecordUnlocks = recordLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.LibraryStuff[(int)MainManager.LibraryPages.Logbook, x.GameId]),
            AreaUnlocks = areaLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.LibraryStuff[(int)MainManager.LibraryPages.Map, x.GameId]),
            Flags = flagLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.Flags[x.GameId]),
            Flagstrings = flagstringLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.Flagstring[x.GameId]),
            Flagvars = flagvarLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.Flagvar[x.GameId]),
            CrystalBerries = crystalBerryLeaves.ToDictionary(
                x => x.NamedId,
                x => _gameDataRuntimeState.CrystalBFlags[x.GameId])
        };
    }

    private static List<TLeaf> GetNormalizedLeavesList<TLeaf>(
        string budId,
        Dictionary<string, List<TLeaf>> leavesByCreatorId)
        where TLeaf : Leaf
    {
        return leavesByCreatorId.TryGetValue(budId, out List<TLeaf> leaves)
            ? leaves
            : [];
    }

    private static Dictionary<string, List<TLeaf>> GetLeavesByCreatorIds<TLeaf>(ILeavesRegistry<TLeaf> leavesRegistry)
        where TLeaf : Leaf
    {
        return leavesRegistry.LeavesByEffectiveIds.Values
            .Where(x => x.CreatorId != Constants.BaseGameCreatorId)
            .GroupBy(x => x.CreatorId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}