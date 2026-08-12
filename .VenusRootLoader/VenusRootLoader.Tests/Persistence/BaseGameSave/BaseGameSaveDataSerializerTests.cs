using NSubstitute;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BaseGameSave;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Tests.Persistence.BaseGameSave;

public sealed class BaseGameSaveDataSerializerTests
{
    private readonly IGameDataRuntimeState _gameDataRuntimeState = Substitute.For<IGameDataRuntimeState>();
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsLeafRegistry = Substitute.For<ILeavesRegistry<AnimIdLeaf>>();
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry = Substitute.For<ILeavesRegistry<AreaLeaf>>();
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry = Substitute.For<ILeavesRegistry<MapLeaf>>();
    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry = Substitute.For<ILeavesRegistry<MedalLeaf>>();
    private readonly ILeavesRegistry<QuestLeaf> _questsLeafRegistry = Substitute.For<ILeavesRegistry<QuestLeaf>>();
    private readonly ILeavesRegistry<ItemLeaf> _itemsLeafRegistry = Substitute.For<ILeavesRegistry<ItemLeaf>>();
    private readonly ILeavesRegistry<MusicLeaf> _musicsLeafRegistry = Substitute.For<ILeavesRegistry<MusicLeaf>>();

    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry =
        Substitute.For<ILeavesRegistry<DiscoveryLeaf>>();

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry =
        Substitute.For<ILeavesRegistry<EnemyLeaf>>();

    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesLeafRegistry =
        Substitute.For<ILeavesRegistry<RecipeLibraryEntryLeaf>>();

    private readonly ILeavesRegistry<RecordLeaf> _recordsLeafRegistry = Substitute.For<ILeavesRegistry<RecordLeaf>>();
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry = Substitute.For<ILeavesRegistry<FlagLeaf>>();

    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry =
        Substitute.For<ILeavesRegistry<FlagstringLeaf>>();

    private readonly ILeavesRegistry<SpyCardLeaf>
        _spyCardsLeafRegistry = Substitute.For<ILeavesRegistry<SpyCardLeaf>>();

    private readonly ILeavesRegistry<FlagvarLeaf>
        _flagvarsLeafRegistry = Substitute.For<ILeavesRegistry<FlagvarLeaf>>();

    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry =
        Substitute.For<ILeavesRegistry<CrystalBerryLeaf>>();

    private readonly BaseGameSaveDataSerializer _sut;

    public BaseGameSaveDataSerializerTests()
    {
        _sut = new(
            _gameDataRuntimeState,
            _animIdsLeafRegistry,
            _mapsLeafRegistry,
            _areasLeafRegistry,
            _medalsLeafRegistry,
            _questsLeafRegistry,
            _itemsLeafRegistry,
            _musicsLeafRegistry,
            _discoveriesLeafRegistry,
            _enemiesLeafRegistry,
            _recipeLibraryEntriesLeafRegistry,
            _recordsLeafRegistry,
            _flagsLeafRegistry,
            _flagstringsLeafRegistry,
            _spyCardsLeafRegistry,
            _flagvarsLeafRegistry,
            _crystalBerriesLeafRegistry);
    }

    [Fact]
    public Task GetBaseGameSaveDataFromRuntimeState_ReturnsSerialisedBaseGameSaveData_WhenRuntimeStateHasMinimalData()
    {
        int flagsAmount = 750;
        int flagstringAmount = 15;
        int flagvarAmount = 70;
        int crystalBerriesAmount = 50;
        int discoveriesAmount = 50;
        int enemiesAmount = 116;
        int recipeLibraryEntriesAmount = 70;
        int recordsAmount = 30;
        int areasAmount = 25;

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        string[] flagstringValues = Enumerable.Repeat("", 100).ToArray();

        _gameDataRuntimeState.PlayerPosition.Returns(new Vector3(4, 5, 6));
        _gameDataRuntimeState.PlayerData.Returns([]);
        _gameDataRuntimeState.MapAreaId.Returns((int)area);
        _gameDataRuntimeState.MapName.Returns(((int)map).ToString());
        _gameDataRuntimeState.PartyLevel.Returns(5);
        _gameDataRuntimeState.PartyExp.Returns(6);
        _gameDataRuntimeState.NeededExp.Returns(7);
        _gameDataRuntimeState.BaseTp.Returns(8);
        _gameDataRuntimeState.Tp.Returns(9);
        _gameDataRuntimeState.Money.Returns(10);
        _gameDataRuntimeState.Bp.Returns(11);
        _gameDataRuntimeState.MaxBp.Returns(12);
        _gameDataRuntimeState.MaxItems.Returns(13);
        _gameDataRuntimeState.MaxStorage.Returns(14);
        _gameDataRuntimeState.ClockHour.Returns(15);
        _gameDataRuntimeState.ClockMin.Returns(16);
        _gameDataRuntimeState.ClockSec.Returns(17);
        _gameDataRuntimeState.AvailableBadgePool.Returns([[], []]);
        _gameDataRuntimeState.BadgeShops.Returns([[], []]);
        _gameDataRuntimeState.BoardQuests.Returns([[], [], []]);
        _gameDataRuntimeState.Items.Returns([[], [], []]);
        _gameDataRuntimeState.Badges.Returns([]);
        _gameDataRuntimeState.SamiraMusics.Returns([]);
        _gameDataRuntimeState.StatBonus.Returns([]);
        _gameDataRuntimeState.LibraryStuff.Returns(new bool[5, 500]);
        _gameDataRuntimeState.Flags.Returns(new bool[1000]);
        _gameDataRuntimeState.Flagstring.Returns(flagstringValues);
        _gameDataRuntimeState.Flagvar.Returns(new int[100]);
        _gameDataRuntimeState.RegionalFlags.Returns(new bool[200]);
        _gameDataRuntimeState.CrystalBFlags.Returns(new bool[100]);
        _gameDataRuntimeState.ExtraFollowers.Returns([]);
        _gameDataRuntimeState.EnemyEncounter.Returns(new int[500, 2]);

        List<MapLeaf> mapLeaves = [new((int)map, Constants.BaseGameCreatorId, map.ToString())];
        TestUtility.MockRegistry(_mapsLeafRegistry, mapLeaves);

        List<DiscoveryLeaf> discoveries = new();
        for (int i = 0; i < discoveriesAmount; i++)
            discoveries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_discoveriesLeafRegistry, discoveries);

        List<EnemyLeaf> enemies = new();
        for (int i = 0; i < enemiesAmount; i++)
            enemies.Add(new(i, Constants.BaseGameCreatorId, ((MainManager.Enemies)i).ToString()));
        TestUtility.MockRegistry(_enemiesLeafRegistry, enemies);

        List<RecipeLibraryEntryLeaf> recipeLibraryEntries = new();
        for (int i = 0; i < recipeLibraryEntriesAmount; i++)
            recipeLibraryEntries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntries);

        List<RecordLeaf> records = new();
        for (int i = 0; i < recordsAmount; i++)
            records.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_recordsLeafRegistry, records);

        List<AreaLeaf> areas = new();
        for (int i = 0; i < areasAmount; i++)
            areas.Add(new(i, Constants.BaseGameCreatorId, ((MainManager.Areas)i).ToString()));
        TestUtility.MockRegistry(_areasLeafRegistry, areas);

        List<FlagLeaf> flagsLeaves = new();
        for (int i = 0; i < flagsAmount; i++)
            flagsLeaves.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagsLeafRegistry, flagsLeaves);

        List<FlagstringLeaf> flagstrings = new();
        for (int i = 0; i < flagstringAmount; i++)
            flagstrings.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagstringsLeafRegistry, flagstrings);

        List<FlagvarLeaf> flagvars = new();
        for (int i = 0; i < flagvarAmount; i++)
            flagvars.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagvarsLeafRegistry, flagvars);

        List<CrystalBerryLeaf> crystalBerries = new();
        for (int i = 0; i < crystalBerriesAmount; i++)
            crystalBerries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_crystalBerriesLeafRegistry, crystalBerries);

        string result = _sut.GetBaseGameSaveDataFromRuntimeState(null);

        _ = _gameDataRuntimeState.Received(1).PlayerPosition;
        _ = _gameDataRuntimeState.Received(1).PlayerData;
        _ = _gameDataRuntimeState.Received(1).MapAreaId;
        _ = _gameDataRuntimeState.Received(1).MapName;
        _ = _gameDataRuntimeState.Received(1).PartyLevel;
        _ = _gameDataRuntimeState.Received(1).PartyExp;
        _ = _gameDataRuntimeState.Received(1).NeededExp;
        _ = _gameDataRuntimeState.Received(1).BaseTp;
        _ = _gameDataRuntimeState.Received(1).Tp;
        _ = _gameDataRuntimeState.Received(1).Money;
        _ = _gameDataRuntimeState.Received(1).Bp;
        _ = _gameDataRuntimeState.Received(1).MaxBp;
        _ = _gameDataRuntimeState.Received(1).MaxItems;
        _ = _gameDataRuntimeState.Received(1).MaxStorage;
        _ = _gameDataRuntimeState.Received(1).ClockHour;
        _ = _gameDataRuntimeState.Received(1).ClockMin;
        _ = _gameDataRuntimeState.Received(1).ClockSec;
        _ = _gameDataRuntimeState.Received(1).AvailableBadgePool;
        _ = _gameDataRuntimeState.Received(1).BadgeShops;
        _ = _gameDataRuntimeState.Received().BoardQuests;
        _ = _gameDataRuntimeState.Received().Badges;
        _ = _gameDataRuntimeState.Received().SamiraMusics;
        _ = _gameDataRuntimeState.Received().StatBonus;
        _ = _gameDataRuntimeState.Received().LibraryStuff;
        _ = _gameDataRuntimeState.Received(flagsAmount + 6 + 7).Flags;
        _ = _gameDataRuntimeState.Received(flagstringAmount + 3 + 1).Flagstring;
        _ = _gameDataRuntimeState.Received(flagvarAmount).Flagvar;
        _ = _gameDataRuntimeState.Received(100).RegionalFlags;
        _ = _gameDataRuntimeState.Received(50).CrystalBFlags;
        _ = _gameDataRuntimeState.Received().ExtraFollowers;
        _ = _gameDataRuntimeState.Received().EnemyEncounter;

        _ = _mapsLeafRegistry.Received(1).GetByGameId((int)map);
        _ = _areasLeafRegistry.ReceivedWithAnyArgs(1).GetByGameId(0);

        return Verify(result);
    }

    [Fact]
    public Task GetBaseGameSaveDataFromRuntimeState_ReturnsSerialisedBaseGameSaveData_WhenRuntimeStateHasFullData()
    {
        Vector3 playerPositionToSave = new(1, 2, 3);

        int flagsAmount = 750;
        int flagstringAmount = 15;
        int flagvarAmount = 70;
        int crystalBerriesAmount = 50;
        int discoveriesAmount = 50;
        int enemiesAmount = 116;
        int recipeLibraryEntriesAmount = 70;
        int recordsAmount = 30;
        int areasAmount = 25;

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        MainManager.BadgeTypes medalMerab1 = MainManager.BadgeTypes.DoublePain;
        MainManager.BadgeTypes medalMerab2 = MainManager.BadgeTypes.DoublePainReal;
        MainManager.BadgeTypes medalShades1 = MainManager.BadgeTypes.BerryFinder;
        MainManager.BadgeTypes medalShades2 = MainManager.BadgeTypes.BumpAttack;

        MainManager.BoardQuests openQuest1 = MainManager.BoardQuests.LadybugQuest;
        MainManager.BoardQuests openQuest2 = MainManager.BoardQuests.MenderQuest;

        MainManager.BoardQuests takenQuest1 = MainManager.BoardQuests.ToyQuest;
        MainManager.BoardQuests takenQuest2 = MainManager.BoardQuests.BadBook;

        MainManager.BoardQuests completedQuest1 = MainManager.BoardQuests.Bomby;
        MainManager.BoardQuests completedQuest2 = MainManager.BoardQuests.ArtBee;

        MainManager.Items regularItem1 = MainManager.Items.AntCompass;
        MainManager.Items regularItem2 = MainManager.Items.BerryJuice;

        MainManager.Items keyItem1 = MainManager.Items.BerryShake;
        MainManager.Items keyItem2 = MainManager.Items.BadBook;

        MainManager.Items storedItem1 = MainManager.Items.ClearWater;
        MainManager.Items storedItem2 = MainManager.Items.Coffee;

        MainManager.BadgeTypes medalOnHandEquippedToMember = MainManager.BadgeTypes.ChargeUp;
        MainManager.BadgeTypes medalOnHandEquippedToParty = MainManager.BadgeTypes.DefenseExchange;
        MainManager.BadgeTypes medalOnHandUnequipped = MainManager.BadgeTypes.PoisonAttacker;

        MainManager.Musics samiraSongNotBought = MainManager.Musics.Battle0;
        MainManager.Musics samiraSongBought = MainManager.Musics.Field2;

        MainManager.AnimIDs follower1 = MainManager.AnimIDs.OldMoth;
        MainManager.AnimIDs follower2 = MainManager.AnimIDs.AntQueen;

        bool[,] libraryStuff = new bool[5, 500];
        for (int i = 0; i < libraryStuff.GetLength(0); i++)
        for (int j = 0; j < libraryStuff.GetLength(1); j++)
            libraryStuff[i, j] = true;

        libraryStuff[0, 1] = false;
        libraryStuff[1, 2] = false;
        libraryStuff[2, 3] = false;
        libraryStuff[3, 4] = false;
        libraryStuff[4, 5] = false;

        bool[] flags = Enumerable.Repeat(true, 1000).ToArray();
        flags[4] = false;
        flags[656] = false;
        flags[345] = false;
        flags[346] = false;
        flags[347] = false;
        flags[555] = false;

        bool[] regionalFlags = Enumerable.Repeat(true, 200).ToArray();
        regionalFlags[3] = false;

        bool[] crystalBerriesObtained = Enumerable.Repeat(true, 100).ToArray();
        crystalBerriesObtained[1] = false;

        MainManager.BadgeTypes medalMystery1 = MainManager.BadgeTypes.EXPBoost;
        MainManager.BadgeTypes medalMystery2 = MainManager.BadgeTypes.ShockTrooper;
        string[] flagstringValues = Enumerable.Repeat("abc", 100).ToArray();
        flagstringValues[8] = $"{(int)regularItem1},{(int)regularItem2}-{(int)keyItem1},{(int)keyItem2}-789";
        flagstringValues[10] = "SomeFilename";
        flagstringValues[12] = "5,6";
        flagstringValues[13] = $"{(int)medalMystery1},{(int)medalMystery2}";

        MainManager.Items chompyItem = MainManager.Items.HoneyDrop;
        int[] flagvarValues = Enumerable.Repeat(69, 200).ToArray();
        flagvarValues[2] = 5;
        flagvarValues[56] = (int)chompyItem;

        int[,] enemyEncounters = new int[500, 2];
        for (int i = 0; i < enemyEncounters.GetLength(0); i++)
        for (int j = 0; j < enemyEncounters.GetLength(1); j++)
            enemyEncounters[i, j] = 5;
        enemyEncounters[5, 0] = 1;
        enemyEncounters[5, 1] = 2;

        _gameDataRuntimeState.PlayerPosition.Returns(new Vector3(4, 5, 6));
        _gameDataRuntimeState.PlayerData.Returns(
        [
            new()
            {
                Trueid = 0,
                Hp = 1,
                Maxhp = 2,
                Basehp = 3,
                Atk = 4,
                Baseatk = 5,
                Def = 6,
                Basedef = 7
            },
            new()
            {
                Trueid = 1,
                Hp = 8,
                Maxhp = 9,
                Basehp = 10,
                Atk = 11,
                Baseatk = 12,
                Def = 13,
                Basedef = 14
            }
        ]);
        _gameDataRuntimeState.MapAreaId.Returns((int)area);
        _gameDataRuntimeState.MapName.Returns(((int)map).ToString());
        _gameDataRuntimeState.PartyLevel.Returns(5);
        _gameDataRuntimeState.PartyExp.Returns(6);
        _gameDataRuntimeState.NeededExp.Returns(7);
        _gameDataRuntimeState.BaseTp.Returns(8);
        _gameDataRuntimeState.Tp.Returns(9);
        _gameDataRuntimeState.Money.Returns(10);
        _gameDataRuntimeState.Bp.Returns(11);
        _gameDataRuntimeState.MaxBp.Returns(12);
        _gameDataRuntimeState.MaxItems.Returns(13);
        _gameDataRuntimeState.MaxStorage.Returns(14);
        _gameDataRuntimeState.ClockHour.Returns(15);
        _gameDataRuntimeState.ClockMin.Returns(16);
        _gameDataRuntimeState.ClockSec.Returns(17);
        _gameDataRuntimeState.AvailableBadgePool.Returns(
        [
            [(int)medalMerab1, (int)medalMerab2],
            [(int)medalShades1, (int)medalShades2]
        ]);
        _gameDataRuntimeState.BadgeShops.Returns(
        [
            [(int)medalMerab2, (int)medalMerab1],
            [(int)medalShades2, (int)medalShades1]
        ]);
        _gameDataRuntimeState.BoardQuests.Returns(
        [
            [(int)openQuest1, (int)openQuest2],
            [(int)takenQuest1, (int)takenQuest2],
            [(int)completedQuest1, (int)completedQuest2]
        ]);
        _gameDataRuntimeState.Items.Returns(
        [
            [(int)regularItem1, (int)regularItem2],
            [(int)keyItem1, (int)keyItem2],
            [(int)storedItem1, (int)storedItem2]
        ]);
        _gameDataRuntimeState.Badges.Returns(
        [
            [(int)medalOnHandEquippedToMember, 0],
            [(int)medalOnHandEquippedToParty, -1],
            [(int)medalOnHandUnequipped, -2]
        ]);
        _gameDataRuntimeState.SamiraMusics.Returns(
        [
            [(int)samiraSongNotBought, -1],
            [(int)samiraSongBought, 1]
        ]);
        _gameDataRuntimeState.StatBonus.Returns(
        [
            [0, 1, 1],
            [2, 3, -1]
        ]);
        _gameDataRuntimeState.LibraryStuff.Returns(libraryStuff);
        _gameDataRuntimeState.Flags.Returns(flags);
        _gameDataRuntimeState.Flagstring.Returns(flagstringValues);
        _gameDataRuntimeState.Flagvar.Returns(flagvarValues);
        _gameDataRuntimeState.RegionalFlags.Returns(regionalFlags);
        _gameDataRuntimeState.CrystalBFlags.Returns(crystalBerriesObtained);
        _gameDataRuntimeState.ExtraFollowers.Returns([(int)follower1, (int)follower2]);
        _gameDataRuntimeState.EnemyEncounter.Returns(enemyEncounters);

        _animIdsLeafRegistry.GetByGameId(-1).Returns(
            new AnimIdLeaf(-1, Constants.BaseGameCreatorId, nameof(MainManager.AnimIDs.None)));
        _animIdsLeafRegistry.GetByGameId(0).Returns(
            new AnimIdLeaf(0, Constants.BaseGameCreatorId, nameof(MainManager.AnimIDs.Bee)));
        _animIdsLeafRegistry.GetByGameId(1).Returns(
            new AnimIdLeaf(1, Constants.BaseGameCreatorId, nameof(MainManager.AnimIDs.Beetle)));
        _animIdsLeafRegistry.GetByGameId((int)follower1).Returns(
            new AnimIdLeaf((int)follower1, Constants.BaseGameCreatorId, follower1.ToString()));
        _animIdsLeafRegistry.GetByGameId((int)follower2).Returns(
            new AnimIdLeaf((int)follower2, Constants.BaseGameCreatorId, follower2.ToString()));

        _mapsLeafRegistry.GetByGameId((int)map).Returns(
            new MapLeaf((int)map, Constants.BaseGameCreatorId, map.ToString()));

        _medalsLeafRegistry.GetByGameId((int)medalMerab1).Returns(
            new MedalLeaf((int)medalMerab1, Constants.BaseGameCreatorId, medalMerab1.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalMerab2).Returns(
            new MedalLeaf((int)medalMerab2, Constants.BaseGameCreatorId, medalMerab2.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalShades1).Returns(
            new MedalLeaf((int)medalShades1, Constants.BaseGameCreatorId, medalShades1.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalShades2).Returns(
            new MedalLeaf((int)medalShades2, Constants.BaseGameCreatorId, medalShades2.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalOnHandEquippedToMember).Returns(
            new MedalLeaf(
                (int)medalOnHandEquippedToMember,
                Constants.BaseGameCreatorId,
                medalOnHandEquippedToMember.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalOnHandEquippedToParty).Returns(
            new MedalLeaf(
                (int)medalOnHandEquippedToParty,
                Constants.BaseGameCreatorId,
                medalOnHandEquippedToParty.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalOnHandUnequipped).Returns(
            new MedalLeaf((int)medalOnHandUnequipped, Constants.BaseGameCreatorId, medalOnHandUnequipped.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalMystery1).Returns(
            new MedalLeaf((int)medalMystery1, Constants.BaseGameCreatorId, medalMystery1.ToString()));
        _medalsLeafRegistry.GetByGameId((int)medalMystery2).Returns(
            new MedalLeaf((int)medalMystery2, Constants.BaseGameCreatorId, medalMystery2.ToString()));

        _questsLeafRegistry.GetByGameId((int)openQuest1).Returns(
            new QuestLeaf((int)openQuest1, Constants.BaseGameCreatorId, openQuest1.ToString()));
        _questsLeafRegistry.GetByGameId((int)openQuest2).Returns(
            new QuestLeaf((int)openQuest2, Constants.BaseGameCreatorId, openQuest2.ToString()));
        _questsLeafRegistry.GetByGameId((int)takenQuest1).Returns(
            new QuestLeaf((int)takenQuest1, Constants.BaseGameCreatorId, takenQuest1.ToString()));
        _questsLeafRegistry.GetByGameId((int)takenQuest2).Returns(
            new QuestLeaf((int)takenQuest2, Constants.BaseGameCreatorId, takenQuest2.ToString()));
        _questsLeafRegistry.GetByGameId((int)completedQuest1).Returns(
            new QuestLeaf((int)completedQuest1, Constants.BaseGameCreatorId, completedQuest1.ToString()));
        _questsLeafRegistry.GetByGameId((int)completedQuest2).Returns(
            new QuestLeaf((int)completedQuest2, Constants.BaseGameCreatorId, completedQuest2.ToString()));

        _itemsLeafRegistry.GetByGameId((int)regularItem1).Returns(
            new ItemLeaf((int)regularItem1, Constants.BaseGameCreatorId, regularItem1.ToString()));
        _itemsLeafRegistry.GetByGameId((int)regularItem2).Returns(
            new ItemLeaf((int)regularItem2, Constants.BaseGameCreatorId, regularItem2.ToString()));
        _itemsLeafRegistry.GetByGameId((int)keyItem1).Returns(
            new ItemLeaf((int)keyItem1, Constants.BaseGameCreatorId, keyItem1.ToString()));
        _itemsLeafRegistry.GetByGameId((int)keyItem2).Returns(
            new ItemLeaf((int)keyItem2, Constants.BaseGameCreatorId, keyItem2.ToString()));
        _itemsLeafRegistry.GetByGameId((int)storedItem1).Returns(
            new ItemLeaf((int)storedItem1, Constants.BaseGameCreatorId, storedItem1.ToString()));
        _itemsLeafRegistry.GetByGameId((int)storedItem2).Returns(
            new ItemLeaf((int)storedItem2, Constants.BaseGameCreatorId, storedItem2.ToString()));
        _itemsLeafRegistry.GetByGameId((int)chompyItem).Returns(
            new ItemLeaf((int)chompyItem, Constants.BaseGameCreatorId, chompyItem.ToString()));

        _musicsLeafRegistry.GetByGameId((int)samiraSongNotBought).Returns(
            new MusicLeaf((int)samiraSongNotBought, Constants.BaseGameCreatorId, samiraSongNotBought.ToString()));
        _musicsLeafRegistry.GetByGameId((int)samiraSongBought).Returns(
            new MusicLeaf((int)samiraSongBought, Constants.BaseGameCreatorId, samiraSongBought.ToString()));

        List<DiscoveryLeaf> discoveries = new();
        for (int i = 0; i < discoveriesAmount; i++)
            discoveries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_discoveriesLeafRegistry, discoveries);

        List<EnemyLeaf> enemies = new();
        for (int i = 0; i < enemiesAmount; i++)
            enemies.Add(new(i, Constants.BaseGameCreatorId, ((MainManager.Enemies)i).ToString()));
        TestUtility.MockRegistry(_enemiesLeafRegistry, enemies);

        List<RecipeLibraryEntryLeaf> recipeLibraryEntries = new();
        for (int i = 0; i < recipeLibraryEntriesAmount; i++)
            recipeLibraryEntries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntries);

        List<RecordLeaf> records = new();
        for (int i = 0; i < recordsAmount; i++)
            records.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_recordsLeafRegistry, records);

        List<AreaLeaf> areas = new();
        for (int i = 0; i < areasAmount; i++)
            areas.Add(new(i, Constants.BaseGameCreatorId, ((MainManager.Areas)i).ToString()));
        TestUtility.MockRegistry(_areasLeafRegistry, areas);

        List<FlagLeaf> flagsLeaves = new();
        for (int i = 0; i < flagsAmount; i++)
            flagsLeaves.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagsLeafRegistry, flagsLeaves);

        _spyCardsLeafRegistry.GetByGameId(5).Returns(
            new SpyCardLeaf(5, Constants.BaseGameCreatorId, "5Card"));
        _spyCardsLeafRegistry.GetByGameId(6).Returns(
            new SpyCardLeaf(6, Constants.BaseGameCreatorId, "6Card"));

        List<FlagstringLeaf> flagstrings = new();
        for (int i = 0; i < flagstringAmount; i++)
            flagstrings.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagstringsLeafRegistry, flagstrings);

        List<FlagvarLeaf> flagvars = new();
        for (int i = 0; i < flagvarAmount; i++)
            flagvars.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_flagvarsLeafRegistry, flagvars);

        List<CrystalBerryLeaf> crystalBerries = new();
        for (int i = 0; i < crystalBerriesAmount; i++)
            crystalBerries.Add(new(i, Constants.BaseGameCreatorId, i.ToString()));
        TestUtility.MockRegistry(_crystalBerriesLeafRegistry, crystalBerries);

        string result = _sut.GetBaseGameSaveDataFromRuntimeState(playerPositionToSave);

        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().PlayerPosition;
        _ = _gameDataRuntimeState.Received().PlayerData;
        _ = _gameDataRuntimeState.Received(1).MapAreaId;
        _ = _gameDataRuntimeState.Received(1).MapName;
        _ = _gameDataRuntimeState.Received(1).PartyLevel;
        _ = _gameDataRuntimeState.Received(1).PartyExp;
        _ = _gameDataRuntimeState.Received(1).NeededExp;
        _ = _gameDataRuntimeState.Received(1).BaseTp;
        _ = _gameDataRuntimeState.Received(1).Tp;
        _ = _gameDataRuntimeState.Received(1).Money;
        _ = _gameDataRuntimeState.Received(1).Bp;
        _ = _gameDataRuntimeState.Received(1).MaxBp;
        _ = _gameDataRuntimeState.Received(1).MaxItems;
        _ = _gameDataRuntimeState.Received(1).MaxStorage;
        _ = _gameDataRuntimeState.Received(1).ClockHour;
        _ = _gameDataRuntimeState.Received(1).ClockMin;
        _ = _gameDataRuntimeState.Received(1).ClockSec;
        _ = _gameDataRuntimeState.Received(1).AvailableBadgePool;
        _ = _gameDataRuntimeState.Received(1).BadgeShops;
        _ = _gameDataRuntimeState.Received().BoardQuests;
        _ = _gameDataRuntimeState.Received().Badges;
        _ = _gameDataRuntimeState.Received().SamiraMusics;
        _ = _gameDataRuntimeState.Received().StatBonus;
        _ = _gameDataRuntimeState.Received().LibraryStuff;
        _ = _gameDataRuntimeState.Received(flagsAmount + 6 + 7).Flags;
        _ = _gameDataRuntimeState.Received(flagstringAmount + 3 + 1).Flagstring;
        _ = _gameDataRuntimeState.Received(flagvarAmount).Flagvar;
        _ = _gameDataRuntimeState.Received(100).RegionalFlags;
        _ = _gameDataRuntimeState.Received(50).CrystalBFlags;
        _ = _gameDataRuntimeState.Received().ExtraFollowers;
        _ = _gameDataRuntimeState.Received().EnemyEncounter;

        _ = _animIdsLeafRegistry.Received(2).GetByGameId(-1);
        _ = _animIdsLeafRegistry.Received(2).GetByGameId(0);
        _ = _animIdsLeafRegistry.Received(2).GetByGameId(1);
        _ = _animIdsLeafRegistry.Received(1).GetByGameId((int)follower1);
        _ = _animIdsLeafRegistry.Received(1).GetByGameId((int)follower2);

        _ = _mapsLeafRegistry.Received(1).GetByGameId((int)map);

        _ = _medalsLeafRegistry.Received(2).GetByGameId((int)medalMerab1);
        _ = _medalsLeafRegistry.Received(2).GetByGameId((int)medalMerab2);
        _ = _medalsLeafRegistry.Received(2).GetByGameId((int)medalShades1);
        _ = _medalsLeafRegistry.Received(2).GetByGameId((int)medalShades2);
        _ = _medalsLeafRegistry.Received(1).GetByGameId((int)medalOnHandEquippedToMember);
        _ = _medalsLeafRegistry.Received(1).GetByGameId((int)medalOnHandEquippedToParty);
        _ = _medalsLeafRegistry.Received(1).GetByGameId((int)medalOnHandUnequipped);
        _ = _medalsLeafRegistry.Received(1).GetByGameId((int)medalMystery1);
        _ = _medalsLeafRegistry.Received(1).GetByGameId((int)medalMystery2);

        _ = _questsLeafRegistry.Received(1).GetByGameId((int)openQuest1);
        _ = _questsLeafRegistry.Received(1).GetByGameId((int)openQuest2);
        _ = _questsLeafRegistry.Received(1).GetByGameId((int)takenQuest1);
        _ = _questsLeafRegistry.Received(1).GetByGameId((int)takenQuest2);
        _ = _questsLeafRegistry.Received(1).GetByGameId((int)completedQuest1);
        _ = _questsLeafRegistry.Received(1).GetByGameId((int)completedQuest2);

        _ = _itemsLeafRegistry.Received(2).GetByGameId((int)regularItem1);
        _ = _itemsLeafRegistry.Received(2).GetByGameId((int)regularItem2);
        _ = _itemsLeafRegistry.Received(2).GetByGameId((int)keyItem1);
        _ = _itemsLeafRegistry.Received(2).GetByGameId((int)keyItem2);
        _ = _itemsLeafRegistry.Received(1).GetByGameId((int)storedItem1);
        _ = _itemsLeafRegistry.Received(1).GetByGameId((int)storedItem2);
        _ = _itemsLeafRegistry.Received(1).GetByGameId((int)chompyItem);

        _ = _musicsLeafRegistry.Received(1).GetByGameId((int)samiraSongNotBought);
        _ = _musicsLeafRegistry.Received(1).GetByGameId((int)samiraSongBought);

        _ = _spyCardsLeafRegistry.Received(1).GetByGameId(5);
        _ = _spyCardsLeafRegistry.Received(1).GetByGameId(6);

        _ = _areasLeafRegistry.ReceivedWithAnyArgs(1).GetByGameId(0);

        return Verify(result);
    }
}