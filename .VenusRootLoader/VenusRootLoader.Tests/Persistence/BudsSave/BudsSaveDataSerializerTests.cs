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
        List<MedalShopLeaf> medalShopLeaves = new() { new(0, Constants.BaseGameCreatorId, "Merab") };
        TestUtility.MockRegistry(_medalShopsLeafRegistry, medalShopLeaves);
        List<MedalLeaf> medalLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.BadgeTypes.HPPlus))
        };
        TestUtility.MockRegistry(_medalsLeafRegistry, medalLeaves);
        List<DiscoveryLeaf> discoveryLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_discoveriesLeafRegistry, discoveryLeaves);
        List<EnemyLeaf> enemyLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.Enemies.CordycepsAnt))
        };
        TestUtility.MockRegistry(_enemiesLeafRegistry, enemyLeaves);
        List<RecipeLibraryEntryLeaf> recipeLibraryEntryLeaves =
            new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntryLeaves);
        List<RecordLeaf> recordLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_recordsLeafRegistry, recordLeaves);
        List<AreaLeaf> areaLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.Areas.BugariaOutskirts))
        };
        TestUtility.MockRegistry(_areasLeafRegistry, areaLeaves);
        List<FlagLeaf> flagLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_flagsLeafRegistry, flagLeaves);
        List<FlagstringLeaf> flagstringLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_flagstringsLeafRegistry, flagstringLeaves);
        List<FlagvarLeaf> flagvarLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_flagvarsLeafRegistry, flagvarLeaves);
        List<CrystalBerryLeaf> crystalBerryLeaves = new() { new(0, Constants.BaseGameCreatorId, "0") };
        TestUtility.MockRegistry(_crystalBerriesLeafRegistry, crystalBerryLeaves);

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

        List<MedalShopLeaf> medalShopLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "Merab"),
            new(1, budId1, "CustomShop")
        };
        TestUtility.MockRegistry(_medalShopsLeafRegistry, medalShopLeaves);
        List<MedalLeaf> medalLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.BadgeTypes.HPPlus)),
            new(1, budId1, "CustomMedal1"),
            new(2, budId1, "CustomMedal2")
        };
        TestUtility.MockRegistry(_medalsLeafRegistry, medalLeaves);
        List<DiscoveryLeaf> discoveryLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId1, "CustomDiscovery")
        };
        TestUtility.MockRegistry(_discoveriesLeafRegistry, discoveryLeaves);
        List<EnemyLeaf> enemyLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.Enemies.CordycepsAnt)),
            new(1, budId2, "CustomEnemy")
        };
        TestUtility.MockRegistry(_enemiesLeafRegistry, enemyLeaves);
        List<RecipeLibraryEntryLeaf> recipeLibraryEntryLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId2, "CustomRecipeLibraryEntry")
        };
        TestUtility.MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntryLeaves);
        List<RecordLeaf> recordLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId3, "CustomRecord")
        };
        TestUtility.MockRegistry(_recordsLeafRegistry, recordLeaves);
        List<AreaLeaf> areaLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, nameof(MainManager.Areas.BugariaOutskirts)),
            new(1, budId3, "CustomArea")
        };
        TestUtility.MockRegistry(_areasLeafRegistry, areaLeaves);
        List<FlagLeaf> flagLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId4, "CustomFlag")
        };
        TestUtility.MockRegistry(_flagsLeafRegistry, flagLeaves);
        List<FlagstringLeaf> flagstringLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId4, "CustomFlagstring")
        };
        TestUtility.MockRegistry(_flagstringsLeafRegistry, flagstringLeaves);
        List<FlagvarLeaf> flagvarLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId5, "CustomFlagvar")
        };
        TestUtility.MockRegistry(_flagvarsLeafRegistry, flagvarLeaves);
        List<CrystalBerryLeaf> crystalBerryLeaves = new()
        {
            new(0, Constants.BaseGameCreatorId, "0"),
            new(1, budId5, "CustomCrystalBerry")
        };
        TestUtility.MockRegistry(_crystalBerriesLeafRegistry, crystalBerryLeaves);

        Dictionary<string, string> result = _sut.GetBudsSaveDataFromRuntimeState();

        _ = _gameDataRuntimeState.Received(1).AvailableBadgePool;
        _ = _gameDataRuntimeState.Received(1).BadgeShops;
        _ = _gameDataRuntimeState.Received(5).LibraryStuff;
        _ = _gameDataRuntimeState.Received(2).EnemyEncounter;
        _ = _gameDataRuntimeState.Received(1).Flags;
        _ = _gameDataRuntimeState.Received(1).Flagstring;
        _ = _gameDataRuntimeState.Received(1).Flagvar;
        _ = _gameDataRuntimeState.Received(1).CrystalBFlags;

        return Verify(result);
    }
}