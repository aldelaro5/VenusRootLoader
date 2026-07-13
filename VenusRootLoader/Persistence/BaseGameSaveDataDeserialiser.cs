using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Persistence;

internal sealed class BaseGameSaveDataDeserialiser : IBaseGameSaveDataDeserialiser
{
    private const char Comma = ',';
    private const char Dash = '-';

    private readonly ILogger<BaseGameSaveDataDeserialiser> _logger;
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsLeafRegistry;
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry;
    private readonly ILeavesRegistry<QuestLeaf> _questsLeafRegistry;
    private readonly ILeavesRegistry<ItemLeaf> _itemsLeafRegistry;
    private readonly ILeavesRegistry<MusicLeaf> _musicsLeafRegistry;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesLeafRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsLeafRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry;
    private readonly ILeavesRegistry<SpyCardLeaf> _spyCardsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry;

    public BaseGameSaveDataDeserialiser(
        ILogger<BaseGameSaveDataDeserialiser> logger,
        ILeavesRegistry<AnimIdLeaf> animIdsLeafRegistry,
        ILeavesRegistry<MapLeaf> mapsLeafRegistry,
        ILeavesRegistry<AreaLeaf> areasLeafRegistry,
        ILeavesRegistry<MedalLeaf> medalsLeafRegistry,
        ILeavesRegistry<QuestLeaf> questsLeafRegistry,
        ILeavesRegistry<ItemLeaf> itemsLeafRegistry,
        ILeavesRegistry<MusicLeaf> musicsLeafRegistry,
        ILeavesRegistry<DiscoveryLeaf> discoveriesLeafRegistry,
        ILeavesRegistry<EnemyLeaf> enemiesLeafRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesLeafRegistry,
        ILeavesRegistry<RecordLeaf> recordsLeafRegistry,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsLeafRegistry,
        ILeavesRegistry<SpyCardLeaf> spyCardsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry,
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesLeafRegistry)
    {
        _logger = logger;
        _animIdsLeafRegistry = animIdsLeafRegistry;
        _mapsLeafRegistry = mapsLeafRegistry;
        _areasLeafRegistry = areasLeafRegistry;
        _medalsLeafRegistry = medalsLeafRegistry;
        _questsLeafRegistry = questsLeafRegistry;
        _itemsLeafRegistry = itemsLeafRegistry;
        _musicsLeafRegistry = musicsLeafRegistry;
        _discoveriesLeafRegistry = discoveriesLeafRegistry;
        _enemiesLeafRegistry = enemiesLeafRegistry;
        _recipeLibraryEntriesLeafRegistry = recipeLibraryEntriesLeafRegistry;
        _recordsLeafRegistry = recordsLeafRegistry;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagstringsLeafRegistry = flagstringsLeafRegistry;
        _spyCardsLeafRegistry = spyCardsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
        _crystalBerriesLeafRegistry = crystalBerriesLeafRegistry;
    }

    public MainManager.LoadData DeserialiseLiteBaseGameSaveData(string saveData)
    {
        string[] baseGameSaveDataLines = saveData.Split(StringUtils.NewlineSplitDelimiter);
        if (baseGameSaveDataLines.Length < 3)
            ThrowHelper.ThrowInvalidDataException("There are less than 3 lines in the base game save data");

        MainManager.LoadData loadData = new();
        LoadHeaderLine(baseGameSaveDataLines[0], ref loadData);
        LoadGeneralInformationLine(baseGameSaveDataLines[2], true, ref loadData);

        return loadData;
    }

    public MainManager.LoadData DeserialiseFullBaseGameSaveData(string saveData)
    {
        string[] baseGameSaveDataLines = saveData.Split(StringUtils.NewlineSplitDelimiter);
        if (baseGameSaveDataLines.Length < 18)
            ThrowHelper.ThrowInvalidDataException("There are less than 18 lines in the base game save data");

        MainManager.LoadData loadData = new();
        LoadHeaderLine(baseGameSaveDataLines[0], ref loadData);
        LoadPlayerPartyLine(baseGameSaveDataLines[1]);
        LoadGeneralInformationLine(baseGameSaveDataLines[2], false, ref loadData);
        MainManager.instance.avaliablebadgepool = LoadMedalShopsLine(baseGameSaveDataLines[3]);
        MainManager.instance.badgeshops = LoadMedalShopsLine(baseGameSaveDataLines[4]);
        LoadQuestsLine(baseGameSaveDataLines[5]);
        LoadItemsLine(baseGameSaveDataLines[6]);
        LoadMedalsLine(baseGameSaveDataLines[7]);
        LoadSamiraSongsLine(baseGameSaveDataLines[8]);
        LoadStatBonusesLine(baseGameSaveDataLines[9]);
        LoadLibraryLine(baseGameSaveDataLines[10]);
        LoadFlagsLine(baseGameSaveDataLines[11]);
        LoadFlagstringsLine(baseGameSaveDataLines[12]);
        LoadFlagvarsLine(baseGameSaveDataLines[13]);
        LoadRegionalFlagsLine(baseGameSaveDataLines[14]);
        LoadCrystalBerriesLine(baseGameSaveDataLines[15]);
        LoadFollowersLine(baseGameSaveDataLines[16]);
        LoadEnemyEncountersDataLine(baseGameSaveDataLines[17]);

        return loadData;
    }

    private static void LoadHeaderLine(string headerLine, ref MainManager.LoadData loadData)
    {
        string[] headerData = headerLine.Split(StringUtils.CommaSplitDelimiter);
        if (headerData.Length < 10)
        {
            ThrowHelper.ThrowInvalidDataException(
                "There are less than 10 fields in the base game save data header line");
        }

        loadData.loadpos = new(
            float.Parse(headerData[0], CultureInfo.InvariantCulture),
            float.Parse(headerData[1], CultureInfo.InvariantCulture),
            float.Parse(headerData[2], CultureInfo.InvariantCulture));

        loadData.challenges =
        [
            bool.Parse(headerData[3]),
            bool.Parse(headerData[4]),
            bool.Parse(headerData[5]),
            bool.Parse(headerData[6]),
            bool.Parse(headerData[7]),
            bool.Parse(headerData[8]),
        ];

        loadData.filename = headerData[9];
    }

    private void LoadPlayerPartyLine(string playerPartyLine)
    {
        string[] playerPartyData = playerPartyLine.Split(StringUtils.AtSymbolSplitDelimiter);
        List<MainManager.BattleData> playerData = new();
        for (int i = 0; i < playerPartyData.Length; i++)
        {
            string[] partyMemberData = playerPartyData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            if (partyMemberData.Length < 8)
            {
                ThrowHelper.ThrowInvalidDataException(
                    $"There are less than 8 fields in the base game save data player party line element index {i}");
            }

            string animIdNamedId = partyMemberData[0];
            if (!_animIdsLeafRegistry.LeavesByNamedIds.TryGetValue(animIdNamedId, out AnimIdLeaf animIdLeaf))
            {
                _logger.LogWarning(
                    "The player party member index {index} has an AnimIdLeaf named {animIdNamedId} while no such " +
                    "AnimIdLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    animIdNamedId);
                continue;
            }

            MainManager.BattleData memberBattleData = new()
            {
                trueid = animIdLeaf.GameId,
                animid = animIdLeaf.GameId,
                hp = int.Parse(partyMemberData[1], CultureInfo.InvariantCulture),
                maxhp = int.Parse(partyMemberData[2], CultureInfo.InvariantCulture),
                basehp = int.Parse(partyMemberData[3], CultureInfo.InvariantCulture),
                atk = int.Parse(partyMemberData[4], CultureInfo.InvariantCulture),
                baseatk = int.Parse(partyMemberData[5], CultureInfo.InvariantCulture),
                def = int.Parse(partyMemberData[6], CultureInfo.InvariantCulture),
                basedef = int.Parse(partyMemberData[7], CultureInfo.InvariantCulture),
                entityname = MainManager.menutext[46 + animIdLeaf.GameId]
            };
            playerData.Add(memberBattleData);
        }

        MainManager.instance.partyorder = playerData.Select(x => x.trueid).ToArray();
        MainManager.instance.playerdata = playerData.ToArray();
    }

    private void LoadGeneralInformationLine(string generalInformationLine, bool lite, ref MainManager.LoadData loadData)
    {
        string[] generalInformationData = generalInformationLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        if (generalInformationData.Length < 16)
        {
            ThrowHelper.ThrowInvalidDataException(
                "There are less than 16 fields in the base game save data general information line");
        }

        string mapNamedId = generalInformationData[6];
        if (!_mapsLeafRegistry.LeavesByNamedIds.TryGetValue(mapNamedId, out MapLeaf mapLeaf))
        {
            ThrowHelper.ThrowInvalidDataException(
                $"The save was done at a {nameof(MapLeaf)} named {mapNamedId} while no such {nameof(MapLeaf)} exists in the registry.");
        }

        string areaNamedId = generalInformationData[7];
        if (!_areasLeafRegistry.LeavesByNamedIds.TryGetValue(areaNamedId, out AreaLeaf areaLeaf))
        {
            ThrowHelper.ThrowInvalidDataException(
                $"The save was done at an {nameof(AreaLeaf)} named {areaNamedId} while no such {nameof(AreaLeaf)} exists in the registry.");
        }

        loadData.level = int.Parse(generalInformationData[0], CultureInfo.InvariantCulture);
        loadData.mapid = mapLeaf.GameId;
        loadData.areaid = areaLeaf.GameId;
        loadData.timeh = int.Parse(generalInformationData[12], CultureInfo.InvariantCulture);
        loadData.timem = int.Parse(generalInformationData[13], CultureInfo.InvariantCulture);
        loadData.times = int.Parse(generalInformationData[14], CultureInfo.InvariantCulture);
        loadData.progression = int.Parse(generalInformationData[15], CultureInfo.InvariantCulture);

        if (lite)
            return;

        MainManager mainManager = MainManager.instance;
        mainManager.partylevel = loadData.level;
        mainManager.partyexp = int.Parse(generalInformationData[1], CultureInfo.InvariantCulture);
        mainManager.neededexp = int.Parse(generalInformationData[2], CultureInfo.InvariantCulture);
        mainManager.basetp = int.Parse(generalInformationData[3], CultureInfo.InvariantCulture);
        mainManager.tp = int.Parse(generalInformationData[4], CultureInfo.InvariantCulture);
        mainManager.money = int.Parse(generalInformationData[5], CultureInfo.InvariantCulture);
        mainManager.bp = int.Parse(generalInformationData[8], CultureInfo.InvariantCulture);
        mainManager.maxbp = int.Parse(generalInformationData[9], CultureInfo.InvariantCulture);
        mainManager.maxitems = int.Parse(generalInformationData[10], CultureInfo.InvariantCulture);
        mainManager.maxstorage = int.Parse(generalInformationData[11], CultureInfo.InvariantCulture);
        mainManager.clockhour = loadData.timeh;
        mainManager.clockmin = loadData.timem;
        mainManager.clocksec = loadData.times;
        mainManager.areaid = loadData.areaid;
    }

    private List<int>[] LoadMedalShopsLine(string medalShopsLine)
    {
        string[] medalShopsData = medalShopsLine.Split(StringUtils.AtSymbolSplitDelimiter);
        List<List<int>> medalShopsMedalGameIds = new();
        for (int i = 0; i < medalShopsData.Length; i++)
        {
            string[] medalNamedIds = medalShopsData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            List<int> medalGameIds = new();
            for (int j = 0; j < medalNamedIds.Length; j++)
            {
                string medalNamedId = medalNamedIds[j];
                if (!_medalsLeafRegistry.LeavesByNamedIds.TryGetValue(medalNamedId, out MedalLeaf medalLeaf))
                {
                    _logger.LogWarning(
                        "The medal shop index {medalShopIndex} medal index {medalIndex} is named {medalNamedId} " +
                        "while no such MedalLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                        i,
                        j,
                        medalNamedId);
                    continue;
                }

                medalGameIds.Add(medalLeaf.GameId);
            }

            medalShopsMedalGameIds.Add(medalGameIds);
        }

        return medalShopsMedalGameIds.ToArray();
    }

    private void LoadQuestsLine(string questsLine)
    {
        string[] boardQuestData = questsLine.Split(StringUtils.AtSymbolSplitDelimiter);
        List<List<int>> boardQuests = new();
        for (int i = 0; i < boardQuestData.Length; i++)
        {
            string[] questNamedIds = boardQuestData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            string boardName = i switch
            {
                0 => "Open",
                1 => "Taken",
                2 => "Completed",
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(null, $"Unknown board index: {i}")
            };
            List<int> questGameIds = new();
            for (int j = 0; j < questNamedIds.Length; j++)
            {
                string questNamedId = questNamedIds[j];
                if (!_questsLeafRegistry.LeavesByNamedIds.TryGetValue(questNamedId, out QuestLeaf questLeaf))
                {
                    _logger.LogWarning(
                        "The {baordName} quest board quest index {questIndex} is named {questNamedId} " +
                        "while no such QuestLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                        boardName,
                        j,
                        questNamedId);
                    continue;
                }

                questGameIds.Add(questLeaf.GameId);
            }

            boardQuests.Add(questGameIds);
        }

        MainManager.instance.boardquests = boardQuests.ToArray();
    }

    private void LoadItemsLine(string itemsLine)
    {
        string[] itemsInventoryData = itemsLine.Split(StringUtils.AtSymbolSplitDelimiter);
        List<List<int>> itemsInventory = new();
        for (int i = 0; i < itemsInventoryData.Length; i++)
        {
            string[] itemNamedIds = itemsInventoryData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            string inventoryName = i switch
            {
                0 => "regular items",
                1 => "key items",
                2 => "stored items",
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(null, $"Unknown items inventory index: {i}")
            };
            List<int> itemGameIds = new();
            for (int j = 0; j < itemNamedIds.Length; j++)
            {
                string itemNamedId = itemNamedIds[j];
                if (!_itemsLeafRegistry.LeavesByNamedIds.TryGetValue(itemNamedId, out ItemLeaf itemLeaf))
                {
                    _logger.LogWarning(
                        "The {inventoryName} items inventory item index {itemIndex} is named {itemNamedId} " +
                        "while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                        inventoryName,
                        j,
                        itemNamedId);
                    continue;
                }

                itemGameIds.Add(itemLeaf.GameId);
            }

            itemsInventory.Add(itemGameIds);
        }

        MainManager.instance.items = itemsInventory.ToArray();
    }

    private void LoadMedalsLine(string medalsLine)
    {
        string[] medalsOnHandData = medalsLine.Split(
            StringUtils.AtSymbolSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        List<int[]> medalsOnHand = new();
        for (int i = 0; i < medalsOnHandData.Length; i++)
        {
            string[] medalEquipData = medalsOnHandData[i].Split(StringUtils.CommaSplitDelimiter);
            string medalNamedId = medalEquipData[0];
            if (!_medalsLeafRegistry.LeavesByNamedIds.TryGetValue(medalNamedId, out MedalLeaf medalLeaf))
            {
                _logger.LogWarning(
                    "The medal index {medalIndex} is named {medalNamedId} while no such MedalLeaf exists in the registry. " +
                    "It will be skipped, but the save file will still be loaded.",
                    i,
                    medalNamedId);
                continue;
            }

            string? medalEquipAnimIdNamedId = !string.IsNullOrWhiteSpace(medalEquipData[1])
                ? medalEquipData[1]
                : null;
            AnimIdLeaf? animIdLeaf = null;
            if (medalEquipAnimIdNamedId is not null &&
                !_animIdsLeafRegistry.LeavesByNamedIds.TryGetValue(medalEquipAnimIdNamedId, out animIdLeaf))
            {
                _logger.LogWarning(
                    "The medal index {medalIndex} is equipped on someone with an AnimIdLeaf named {animIdNamedId} while " +
                    "no such AnimIdLeaf exists in the registry. It will be left unequipped, but the save file will still be loaded.",
                    i,
                    medalEquipAnimIdNamedId);
                animIdLeaf = null;
            }

            medalsOnHand.Add([medalLeaf.GameId, animIdLeaf?.GameId ?? -2]);
        }

        MainManager.instance.badges = medalsOnHand.ToList();
    }

    private void LoadSamiraSongsLine(string samiraSongsLine)
    {
        string[] samiraSongsData = samiraSongsLine.Split(
            StringUtils.AtSymbolSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        List<int[]> samiraSongs = new();
        for (int i = 0; i < samiraSongsData.Length; i++)
        {
            string[] samiraSongData = samiraSongsData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            string samiraSongNamedId = samiraSongData[0];
            if (!_musicsLeafRegistry.LeavesByNamedIds.TryGetValue(samiraSongNamedId, out MusicLeaf musicLeaf))
            {
                _logger.LogWarning(
                    "The samira song index {samiraSongIndex} is named {samiraSongNamedId} while no such MusicLeaf exists in the registry. " +
                    "It will be skipped, but the save file will still be loaded.",
                    i,
                    samiraSongNamedId);
                continue;
            }

            int songBoughtStatus = int.Parse(samiraSongData[1], CultureInfo.InvariantCulture);
            samiraSongs.Add([musicLeaf.GameId, songBoughtStatus]);
        }

        MainManager.instance.samiramusics = samiraSongs.ToList();
    }

    private void LoadStatBonusesLine(string statBonusesLine)
    {
        string[] statBonusesData = statBonusesLine.Split(
            StringUtils.AtSymbolSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        List<int[]> statBonuses = new();
        for (int i = 0; i < statBonusesData.Length; i++)
        {
            string[] statBonusData = statBonusesData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            string targetAnimidNamedId = statBonusData[2];
            if (!_animIdsLeafRegistry.LeavesByNamedIds.TryGetValue(targetAnimidNamedId, out AnimIdLeaf animIdLeaf))
            {
                _logger.LogWarning(
                    "The stat bonus index {statBonusIndex} index is named {targetAnimidNamedId} while no such AnimIdLeaf " +
                    "exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    targetAnimidNamedId);
                continue;
            }

            int bonusType = int.Parse(statBonusData[0], CultureInfo.InvariantCulture);
            int bonusAmount = int.Parse(statBonusData[1], CultureInfo.InvariantCulture);
            statBonuses.Add([bonusType, bonusAmount, animIdLeaf.GameId]);
        }

        MainManager.instance.statbonus = statBonuses.ToList();
    }

    private void LoadLibraryLine(string libraryLine)
    {
        string[] libraryPagesData = libraryLine.Split(
            StringUtils.AtSymbolSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < libraryPagesData.Length; i++)
        {
            string[] libraryFlagsData = libraryPagesData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            int baseGameAmount = (MainManager.LibraryPages)i switch
            {
                MainManager.LibraryPages.Discoveries => _discoveriesLeafRegistry.LeavesByNamedIds.Values
                    .Count(f => f.CreatorId == Constants.BaseGameId),
                MainManager.LibraryPages.Bestiary => _enemiesLeafRegistry.LeavesByNamedIds.Values
                    .Count(f => f.CreatorId == Constants.BaseGameId),
                MainManager.LibraryPages.Recipes => _recipeLibraryEntriesLeafRegistry.LeavesByNamedIds.Values
                    .Count(f => f.CreatorId == Constants.BaseGameId),
                MainManager.LibraryPages.Logbook => _recordsLeafRegistry.LeavesByNamedIds.Values
                    .Count(f => f.CreatorId == Constants.BaseGameId),
                MainManager.LibraryPages.Map => _areasLeafRegistry.LeavesByNamedIds.Values
                    .Count(f => f.CreatorId == Constants.BaseGameId),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>(null, $"Unknown library page index: {i}")
            };
            for (int j = 0; j < baseGameAmount; j++)
                MainManager.instance.librarystuff[i, j] = bool.Parse(libraryFlagsData[j]);
        }
    }

    private void LoadFlagsLine(string flagsLine)
    {
        string[] flagsData = flagsLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        int baseGameAmount = _flagsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        List<bool> flags = new();
        for (int i = 0; i < baseGameAmount; i++)
            flags.Add(bool.Parse(flagsData[i]));

        MainManager.instance.flags = flags.ToArray();
    }

    private void LoadFlagstringsLine(string flagstringsLine)
    {
        string[] flagstringsData = flagstringsLine.Split(StringUtils.FlagstringSplitDelimiter, StringSplitOptions.None);
        int baseGameAmount = _flagstringsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        List<string> flagstrings = new();
        for (int i = 0; i < baseGameAmount; i++)
        {
            string flagstring = i switch
            {
                8 => GetChapter4CaptureDataFlagstringValue(flagstringsData[i]),
                12 => GetSavedSpyCardsDeckFlagstringValue(flagstringsData[i]),
                13 => GetMysteryMedalsQueueFlagstingValue(flagstringsData[i]),
                _ => flagstringsData[i]
            };
            flagstrings.Add(flagstring);
        }

        MainManager.instance.flagstring = flagstrings.ToArray();
    }

    private string GetChapter4CaptureDataFlagstringValue(string flagstring)
    {
        string[] chapter4CaptureData = flagstring.Split(StringUtils.DashSplitDelimiter);
        string[] regularItemNamedIds = chapter4CaptureData[0].Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        string[] keyItemNamedIds = chapter4CaptureData[1].Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);

        StringBuilder sb = new();
        for (int i = 0; i < regularItemNamedIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            string itemNamedId = regularItemNamedIds[i];
            if (!_itemsLeafRegistry.LeavesByNamedIds.TryGetValue(itemNamedId, out ItemLeaf itemLeaf))
            {
                _logger.LogWarning(
                    "The flagstring 8 (Chapter 4 capture data) regular item index {itemIndex} is named {itemNamedId} " +
                    "while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    itemNamedId);
                continue;
            }

            sb.AppendInvariant(itemLeaf.GameId);
        }

        sb.Append(Dash);

        for (int i = 0; i < keyItemNamedIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            string itemNamedId = keyItemNamedIds[i];
            if (!_itemsLeafRegistry.LeavesByNamedIds.TryGetValue(itemNamedId, out ItemLeaf itemLeaf))
            {
                _logger.LogWarning(
                    "The flagstring 8 (Chapter 4 capture data) key item index {itemIndex} is named {itemNamedId} " +
                    "while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    itemNamedId);
                continue;
            }

            sb.AppendInvariant(itemLeaf.GameId);
        }

        sb.Append(Dash);
        sb.AppendInvariant(chapter4CaptureData[2]);

        return sb.ToString();
    }

    private string GetSavedSpyCardsDeckFlagstringValue(string flagstring)
    {
        string[] spyCardsNamedId = flagstring.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        StringBuilder sb = new();
        for (int i = 0; i < spyCardsNamedId.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            string spyCardNamedId = spyCardsNamedId[i];
            if (!_spyCardsLeafRegistry.LeavesByNamedIds.TryGetValue(spyCardNamedId, out SpyCardLeaf spyCardLeaf))
            {
                _logger.LogWarning(
                    "The flagstring 12 (saved Spy Cards deck) card index {cardIndex} is named {spyCardNamedId} " +
                    "while no such SpyCardLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    spyCardNamedId);
                continue;
            }

            sb.AppendInvariant(spyCardLeaf.GameId);
        }

        return sb.ToString();
    }

    private string GetMysteryMedalsQueueFlagstingValue(string flagstring)
    {
        string[] mysteryMedalsQueue = flagstring.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        StringBuilder sb = new();
        for (int i = 0; i < mysteryMedalsQueue.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            string medalNamedId = mysteryMedalsQueue[i];
            if (!_medalsLeafRegistry.LeavesByNamedIds.TryGetValue(medalNamedId, out MedalLeaf medalLeaf))
            {
                _logger.LogWarning(
                    "The flagstring 13 (MYSTERY? medals queue) contains a MedalLeaf named {medalNamedId} " +
                    "while no such MedalLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    medalNamedId);
                continue;
            }

            sb.AppendInvariant(medalLeaf.GameId);
        }

        return sb.ToString();
    }

    private void LoadFlagvarsLine(string flagvarsLine)
    {
        string[] flagvarsData = flagvarsLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        int baseGameAmount = _flagvarsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        List<int> flagvars = new();
        for (int i = 0; i < baseGameAmount; i++)
        {
            int flagvar = i == 56
                ? GetChompyItemFlagvarValue(flagvarsData[i])
                : int.Parse(flagvarsData[i], CultureInfo.InvariantCulture);
            flagvars.Add(flagvar);
        }

        MainManager.instance.flagvar = flagvars.ToArray();
    }

    private int GetChompyItemFlagvarValue(string flagvar)
    {
        if (string.IsNullOrWhiteSpace(flagvar))
            return 0;

        if (_itemsLeafRegistry.LeavesByNamedIds.TryGetValue(flagvar, out ItemLeaf itemLeaf))
            return itemLeaf.GameId;

        if (int.TryParse(flagvar, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
        {
            _logger.LogWarning(
                "The flagvar 56 (ItemLeaf equipped on Chompy) is named {itemNamedId} while no such ItemLeaf " +
                "exists in the registry. The flagvar will be loaded as is since it is parsable as an integer and " +
                "the save file will still be loaded.",
                flagvar);
            return value;
        }

        _logger.LogWarning(
            "The flagvar 56 (ItemLeaf equipped on Chompy) is named {itemNamedId} while no such ItemLeaf " +
            "exists in the registry. The flagvar will be left with a value of 0 since it is not parsable as " +
            "an integer, but the save file will still be loaded.",
            flagvar);
        return 0;
    }

    private void LoadRegionalFlagsLine(string regionalFlagsLine)
    {
        string[] regionalFlagsData = regionalFlagsLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        int baseGameAmount = 100;
        List<bool> regionalFlags = new();
        for (int i = 0; i < baseGameAmount; i++)
            regionalFlags.Add(bool.Parse(regionalFlagsData[i]));

        MainManager.instance.regionalflags = regionalFlags.ToArray();
    }

    private void LoadCrystalBerriesLine(string crystalBerriesLine)
    {
        string[] crystalBerriesData = crystalBerriesLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        int baseGameAmount = _crystalBerriesLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        List<bool> crystalBerries = new();
        for (int i = 0; i < baseGameAmount; i++)
            crystalBerries.Add(bool.Parse(crystalBerriesData[i]));

        MainManager.instance.crystalbflags = crystalBerries.ToArray();
    }

    private void LoadFollowersLine(string followersLine)
    {
        string[] animIdNamedIds = followersLine.Split(
            StringUtils.CommaSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        List<int> followerGameIds = new();
        for (int i = 0; i < animIdNamedIds.Length; i++)
        {
            string animIdNamedId = animIdNamedIds[i];
            if (!_animIdsLeafRegistry.LeavesByNamedIds.TryGetValue(animIdNamedId, out AnimIdLeaf animIdLeaf))
            {
                _logger.LogWarning(
                    "The follower index {followerIndex}'s AnimIdLeaf is named {animIdNamedId} while no such " +
                    "AnimIdLeaf exists in the registry. It will be skipped, but the save file will still be loaded.",
                    i,
                    animIdNamedId);
                continue;
            }

            followerGameIds.Add(animIdLeaf.GameId);
        }

        MainManager.instance.extrafollowers = followerGameIds.ToList();
    }

    private void LoadEnemyEncountersDataLine(string enemyEncountersDataLine)
    {
        string[] enemyEncountersData = enemyEncountersDataLine.Split(
            StringUtils.AtSymbolSplitDelimiter,
            StringSplitOptions.RemoveEmptyEntries);
        int baseGameAmount = _enemiesLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            string[] enemyEncounterData = enemyEncountersData[i].Split(
                StringUtils.CommaSplitDelimiter,
                StringSplitOptions.RemoveEmptyEntries);
            int amountSeen = int.Parse(enemyEncounterData[0], CultureInfo.InvariantCulture);
            int amountDefeated = int.Parse(enemyEncounterData[1], CultureInfo.InvariantCulture);
            MainManager.instance.enemyencounter[i, 0] = amountSeen;
            MainManager.instance.enemyencounter[i, 1] = amountDefeated;
        }
    }
}