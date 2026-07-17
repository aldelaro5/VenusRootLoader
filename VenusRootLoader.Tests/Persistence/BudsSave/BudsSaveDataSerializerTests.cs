using AwesomeAssertions;
using NSubstitute;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BudsSave;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Tests.Persistence.BudsSave;

public sealed class BudsSaveDataSerializerTests
{
    private readonly IGameDataRuntimeState _gameDataRuntimeState = Substitute.For<IGameDataRuntimeState>();

    private readonly ILeavesRegistry<MedalShopLeaf> _medalShopsLeafRegistry =
        Substitute.For<ILeavesRegistry<MedalShopLeaf>>();

    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry = Substitute.For<ILeavesRegistry<MedalLeaf>>();

    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry =
        Substitute.For<ILeavesRegistry<DiscoveryLeaf>>();

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry = Substitute.For<ILeavesRegistry<EnemyLeaf>>();

    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesLeafRegistry =
        Substitute.For<ILeavesRegistry<RecipeLibraryEntryLeaf>>();

    private readonly ILeavesRegistry<RecordLeaf> _recordsLeafRegistry = Substitute.For<ILeavesRegistry<RecordLeaf>>();
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry = Substitute.For<ILeavesRegistry<AreaLeaf>>();
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry = Substitute.For<ILeavesRegistry<FlagLeaf>>();

    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry =
        Substitute.For<ILeavesRegistry<FlagstringLeaf>>();

    private readonly ILeavesRegistry<FlagvarLeaf>
        _flagvarsLeafRegistry = Substitute.For<ILeavesRegistry<FlagvarLeaf>>();

    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry =
        Substitute.For<ILeavesRegistry<CrystalBerryLeaf>>();

    private readonly BudsSaveDataSerializer _sut;

    public BudsSaveDataSerializerTests()
    {
        _sut = new(
            _gameDataRuntimeState,
            _medalShopsLeafRegistry,
            _medalsLeafRegistry,
            _discoveriesLeafRegistry,
            _enemiesLeafRegistry,
            _recipeLibraryEntriesLeafRegistry,
            _recordsLeafRegistry,
            _areasLeafRegistry,
            _flagsLeafRegistry,
            _flagstringsLeafRegistry,
            _flagvarsLeafRegistry,
            _crystalBerriesLeafRegistry);
    }

    [Fact]
    public void GetBudsSaveDataFromRuntimeState_ReturnsEmptyDictionnary_WhenThereAreNoCustomLeaves()
    {
        _medalShopsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, MedalShopLeaf> { [0] = new(0, "Merab", Constants.BaseGameCreatorId) });
        _medalsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, MedalLeaf>
            {
                [0] = new(0, nameof(MainManager.BadgeTypes.HPPlus), Constants.BaseGameCreatorId)
            });
        _discoveriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, DiscoveryLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _enemiesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, EnemyLeaf>
            {
                [0] = new(0, nameof(MainManager.Enemies.CordycepsAnt), Constants.BaseGameCreatorId)
            });
        _recipeLibraryEntriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, RecipeLibraryEntryLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _recordsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, RecordLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _areasLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, AreaLeaf>
            {
                [0] = new(0, nameof(MainManager.Areas.BugariaOutskirts), Constants.BaseGameCreatorId)
            });
        _flagsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _flagstringsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagstringLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _flagvarsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagvarLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });
        _crystalBerriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, CrystalBerryLeaf> { [0] = new(0, "0", Constants.BaseGameCreatorId) });

        Dictionary<string, string> result = _sut.GetBudsSaveDataFromRuntimeState();

        result.Should().BeEmpty();

        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().AvailableBadgePool;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().BadgeShops;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().LibraryStuff;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().EnemyEncounter;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().Flags;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().Flagstring;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().Flagvar;
        _ = _gameDataRuntimeState.DidNotReceiveWithAnyArgs().CrystalBFlags;

        _ = _medalShopsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _medalsLeafRegistry.DidNotReceiveWithAnyArgs().LeavesByGameIds;
        _ = _discoveriesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _enemiesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _recipeLibraryEntriesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _recordsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _areasLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagstringsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagvarsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _crystalBerriesLeafRegistry.Received(1).LeavesByGameIds;
    }

    [Fact]
    public Task GetBudsSaveDataFromRuntimeState_ReturnsBudsSaveData_WhenThereAreCustomLeaves()
    {
        string budId1 = "bud1";
        string budId2 = "bud2";
        string budId3 = "bud3";
        string budId4 = "bud4";
        string budId5 = "bud5";

        bool[,] libraryStuff = new bool[5, 2];
        libraryStuff[0, 1] = true;
        libraryStuff[1, 1] = true;
        libraryStuff[2, 1] = true;
        libraryStuff[3, 1] = true;
        libraryStuff[4, 1] = true;

        int[,] enemyEncounter = new int[2, 2];
        enemyEncounter[1, 0] = 1;
        enemyEncounter[1, 1] = 2;

        bool[] flags = [false, true];
        string[] flagsstrings = ["", "SomeValue"];
        int[] flagvars = [0, 5];
        bool[] crystalBerriesObtained = [false, true];

        _gameDataRuntimeState.AvailableBadgePool.Returns([[], [1]]);
        _gameDataRuntimeState.BadgeShops.Returns([[], [2]]);
        _gameDataRuntimeState.LibraryStuff.Returns(libraryStuff);
        _gameDataRuntimeState.EnemyEncounter.Returns(enemyEncounter);
        _gameDataRuntimeState.Flags.Returns(flags);
        _gameDataRuntimeState.Flagstring.Returns(flagsstrings);
        _gameDataRuntimeState.Flagvar.Returns(flagvars);
        _gameDataRuntimeState.CrystalBFlags.Returns(crystalBerriesObtained);

        _medalShopsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, MedalShopLeaf>
            {
                [0] = new(0, "Merab", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomShop", budId1)
            });
        _medalsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, MedalLeaf>
            {
                [0] = new(0, nameof(MainManager.BadgeTypes.HPPlus), Constants.BaseGameCreatorId),
                [1] = new(1, "CustomMedal1", budId1),
                [2] = new(2, "CustomMedal2", budId1)
            });
        _discoveriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, DiscoveryLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomDiscovery", budId1)
            });
        _enemiesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, EnemyLeaf>
            {
                [0] = new(0, nameof(MainManager.Enemies.CordycepsAnt), Constants.BaseGameCreatorId),
                [1] = new(1, "CustomEnemy", budId2)
            });
        _recipeLibraryEntriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, RecipeLibraryEntryLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomRecipeLibraryEntry", budId2)
            });
        _recordsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, RecordLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomRecord", budId3)
            });
        _areasLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, AreaLeaf>
            {
                [0] = new(0, nameof(MainManager.Areas.BugariaOutskirts), Constants.BaseGameCreatorId),
                [1] = new(1, "CustomArea", budId3)
            });
        _flagsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomFlag", budId4)
            });
        _flagstringsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagstringLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomFlagstring", budId4)
            });
        _flagvarsLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, FlagvarLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomFlagvar", budId5)
            });
        _crystalBerriesLeafRegistry.LeavesByGameIds.Returns(
            new Dictionary<int, CrystalBerryLeaf>
            {
                [0] = new(0, "0", Constants.BaseGameCreatorId),
                [1] = new(1, "CustomCrystalBerry", budId5)
            });

        Dictionary<string, string> result = _sut.GetBudsSaveDataFromRuntimeState();

        _ = _gameDataRuntimeState.Received(1).AvailableBadgePool;
        _ = _gameDataRuntimeState.Received(1).BadgeShops;
        _ = _gameDataRuntimeState.Received(5).LibraryStuff;
        _ = _gameDataRuntimeState.Received(2).EnemyEncounter;
        _ = _gameDataRuntimeState.Received(1).Flags;
        _ = _gameDataRuntimeState.Received(1).Flagstring;
        _ = _gameDataRuntimeState.Received(1).Flagvar;
        _ = _gameDataRuntimeState.Received(1).CrystalBFlags;

        _ = _medalShopsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _medalsLeafRegistry.Received(2).LeavesByGameIds;
        _ = _discoveriesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _enemiesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _recipeLibraryEntriesLeafRegistry.Received(1).LeavesByGameIds;
        _ = _recordsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _areasLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagstringsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _flagvarsLeafRegistry.Received(1).LeavesByGameIds;
        _ = _crystalBerriesLeafRegistry.Received(1).LeavesByGameIds;

        return Verify(result);
    }
}