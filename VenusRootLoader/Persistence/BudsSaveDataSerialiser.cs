using System.Text.Json;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Persistence;

internal sealed class BudsSaveDataSerialiser : IBudsSaveDataSerialiser
{
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

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

    public BudsSaveDataSerialiser(
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
                budId,
                medalShopsByCreatorId,
                discoveriesByCreatorId,
                enemiesByCreatorId,
                recipeLibraryEntriesByCreatorId,
                recordsByCreatorId,
                areasByCreatorId,
                flagsByCreatorId,
                flagstringsByCreatorId,
                flagvarsByCreatorId,
                crystalBerriesByCreatorId);
            string jsonBudSaveData = JsonSerializer.Serialize(budSaveData, _serializerOptions);
            budsSaveData.Add(budId, jsonBudSaveData);
        }

        return budsSaveData;
    }

    private BudSaveData GetBudSaveDataFromRuntimeState(
        string budId,
        Dictionary<string, List<MedalShopLeaf>> medalShopsByCreatorId,
        Dictionary<string, List<DiscoveryLeaf>> discoveriesByCreatorId,
        Dictionary<string, List<EnemyLeaf>> enemiesByCreatorId,
        Dictionary<string, List<RecipeLibraryEntryLeaf>> recipeLibraryEntriesByCreatorId,
        Dictionary<string, List<RecordLeaf>> recordsByCreatorId,
        Dictionary<string, List<AreaLeaf>> areasByCreatorId,
        Dictionary<string, List<FlagLeaf>> flagsByCreatorId,
        Dictionary<string, List<FlagstringLeaf>> flagstringsByCreatorId,
        Dictionary<string, List<FlagvarLeaf>> flagvarsByCreatorId,
        Dictionary<string, List<CrystalBerryLeaf>> crystalBerriesByCreatorId)
    {
        return new()
        {
            MedalShops = medalShopsByCreatorId[budId].ToDictionary(
                medalShopLeaf => medalShopLeaf.NamedId,
                medalShopLeaf => new MedalShopLeafSaveData
                {
                    AvailablePool = MainManager.instance.avaliablebadgepool[medalShopLeaf.GameId]
                        .Select(medalGameId => _medalsLeafRegistry.LeavesByGameIds[medalGameId].NamedId)
                        .ToList(),
                    ShopStock = MainManager.instance.badgeshops[medalShopLeaf.GameId]
                        .Select(medalGameId => _medalsLeafRegistry.LeavesByGameIds[medalGameId].NamedId)
                        .ToList()
                }),
            DiscoveryUnlocks = discoveriesByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.librarystuff[(int)MainManager.LibraryPages.Discoveries, x.GameId]),
            Enemies = enemiesByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => new EnemySaveData
                {
                    IsBestiaryEntryUnlocked =
                        MainManager.instance.librarystuff[(int)MainManager.LibraryPages.Bestiary, x.GameId],
                    AmountSeen = MainManager.instance.enemyencounter[x.GameId, 0],
                    AmountDefeated = MainManager.instance.enemyencounter[x.GameId, 1]
                }),
            RecipeLibraryEntryUnlocks = recipeLibraryEntriesByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.librarystuff[(int)MainManager.LibraryPages.Recipes, x.GameId]),
            RecordUnlocks = recordsByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.librarystuff[(int)MainManager.LibraryPages.Logbook, x.GameId]),
            AreaUnlocks = areasByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.librarystuff[(int)MainManager.LibraryPages.Map, x.GameId]),
            Flags = flagsByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.flags[x.GameId]),
            Flagstrings = flagstringsByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.flagstring[x.GameId]),
            Flagvars = flagvarsByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.flagvar[x.GameId]),
            CrystalBerries = crystalBerriesByCreatorId[budId].ToDictionary(
                x => x.NamedId,
                x => MainManager.instance.crystalbflags[x.GameId])
        };
    }

    private static Dictionary<string, List<TLeaf>> GetLeavesByCreatorIds<TLeaf>(ILeavesRegistry<TLeaf> leavesRegistry)
        where TLeaf : Leaf
    {
        return leavesRegistry.LeavesByNamedIds.Values
            .Where(x => x.CreatorId != Constants.BaseGameId)
            .GroupBy(x => x.CreatorId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}