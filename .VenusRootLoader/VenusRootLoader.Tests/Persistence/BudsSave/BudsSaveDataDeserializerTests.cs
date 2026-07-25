using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Text.Json;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BudsSave;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Tests.Persistence.BudsSave;

public sealed class BudsSaveDataDeserializerTests
{
    private readonly FakeLogger<BudsSaveDataDeserializer> _logger = new();

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

    private readonly BudsSaveDataDeserializer _sut;

    public BudsSaveDataDeserializerTests()
    {
        _sut = new(
            _logger,
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
    public void DeserializeBudsSaveData_DoesNothing_WhenThereAreNoBudsSaveData()
    {
        StagingLoadData stagingLoadData = new();
        stagingLoadData.AvaliableBadgePool.Add([1]);
        stagingLoadData.BadgeShops.Add([2]);
        stagingLoadData.LibraryStuff[0].AddRange([false, true]);
        stagingLoadData.LibraryStuff[1].AddRange([true, true]);
        stagingLoadData.LibraryStuff[2].AddRange([false, false]);
        stagingLoadData.LibraryStuff[3].AddRange([true, false]);
        stagingLoadData.LibraryStuff[4].AddRange([false, true]);
        stagingLoadData.Flags.AddRange([true, false]);
        stagingLoadData.Flagstrings.Add("SomeValue");
        stagingLoadData.Flagvars.AddRange([5, 2]);
        stagingLoadData.CrystalBerryFlags.AddRange([true, false]);
        stagingLoadData.EnemyEncounter.Add([1, 2]);

        string beforeDeserializeData = JsonSerializer.Serialize(stagingLoadData);

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);
        string result = JsonSerializer.Serialize(stagingLoadData);

        result.Should().Be(beforeDeserializeData);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void DeserializeBudsSaveData_ThrowsAnException_WhenBudSaveDataDeserializedToNull()
    {
        Dictionary<string, string> budsSaveDataByIds = new() { ["bud1"] = "null" };
        StagingLoadData stagingLoadData = new();

        Action act = () => _sut.DeserializeBudsSaveData(budsSaveDataByIds, stagingLoadData);

        act.Should()
            .Throw<InvalidDataException>()
            .WithMessage("The bud save data of the bud bud1 deserialized to null");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsMedalShop_WhenMedalShopIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingCustomShop";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops =
                    new()
                    {
                        [namedId] = new()
                        {
                            AvailablePool = ["bud1~CustomMedal1", "bud2~CustomMedal2"],
                            ShopStock = ["bud2~CustomMedal2", "bud1~CustomMedal1"]
                        }
                    },
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.AvaliableBadgePool.Should().BeEmpty();
        stagingLoadData.BadgeShops.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(MedalShopLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsMedalShopWithDefaultData_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomShop";

        Dictionary<string, MedalShopLeaf> medalShopLeaves = new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        medalShopLeaves[$"{budId}~{namedId}"].StartingMedalsStock.AddRange(
        [
            new MedalLeaf(0, "Medal1", "Creator1"),
            new MedalLeaf(1, "Medal2", "Creator2")
        ]);
        MockRegistry(_medalShopsLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.AvaliableBadgePool.Should().BeEquivalentTo(
            new List<List<int>>
            {
                new()
                {
                    0,
                    1
                }
            });
        stagingLoadData.BadgeShops.Should().BeEquivalentTo(
            new List<List<int>>
            {
                new()
                {
                    0,
                    1
                }
            });

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(MedalShopLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsMedalInMedalShopsData_WhenMedalIsNotInRegistry()
    {
        string budId1 = "bud1";
        string budId2 = "bud2";
        string namedId = "CustomShop";
        string medalNamedId = "CustomMedal";
        string missingMedalNamedId = "MissingCustomMedal";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId1] = new()
            {
                MedalShops =
                    new()
                    {
                        [namedId] = new()
                        {
                            AvailablePool = [$"{budId1}~{medalNamedId}", $"{budId2}~{missingMedalNamedId}"],
                            ShopStock = [$"{budId2}~{missingMedalNamedId}", $"{budId1}~{medalNamedId}"]
                        }
                    },
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, MedalShopLeaf> medalShopLeaves = new() { [$"{budId1}~{namedId}"] = new(0, namedId, budId1) };
        MockRegistry(_medalShopsLeafRegistry, medalShopLeaves);

        Dictionary<string, MedalLeaf> medalLeaves = new()
        {
            [$"{budId1}~{medalNamedId}"] = new(0, medalNamedId, budId1)
        };
        MockRegistry(_medalsLeafRegistry, medalLeaves);

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.AvaliableBadgePool.Should().BeEquivalentTo(new List<List<int>> { new() { 0 } });
        stagingLoadData.BadgeShops.Should().BeEquivalentTo(new List<List<int>> { new() { 0 } });

        _logger.Collector.Count.Should().Be(2);
        IReadOnlyList<FakeLogRecord> logRecords = _logger.Collector.GetSnapshot();

        logRecords[0].Level.Should().Be(LogLevel.Warning);
        logRecords[0].Message.Should().Be(
            $"The {nameof(MedalShopLeaf)} named {namedId} has a {nameof(MedalLeaf)} named {missingMedalNamedId} " +
            $"created by {budId2} in its available pool while no such MedalLeaf exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
        logRecords[1].Level.Should().Be(LogLevel.Warning);
        logRecords[1].Message.Should().Be(
            $"The {nameof(MedalShopLeaf)} named {namedId} has a {nameof(MedalLeaf)} named {missingMedalNamedId} " +
            $"created by {budId2} in its shop stock while no such MedalLeaf exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsDiscovery_WhenDiscoveryIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingDiscovery";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new() { [namedId] = true },
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.LibraryStuff[0].Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(DiscoveryLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsDiscoveryLocked_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomDiscovery";

        Dictionary<string, DiscoveryLeaf> medalShopLeaves = new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_discoveriesLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.LibraryStuff[0].Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(DiscoveryLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsEnemyy_WhenEnemyyIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingEnemy";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new()
                {
                    [namedId] = new()
                    {
                        IsBestiaryEntryUnlocked = true,
                        AmountSeen = 1,
                        AmountDefeated = 2
                    }
                },
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.LibraryStuff[1].Should().BeEmpty();
        stagingLoadData.EnemyEncounter.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(EnemyLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsEnemyWithDefaultData_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomEnemy";

        Dictionary<string, EnemyLeaf> medalShopLeaves = new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_enemiesLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.LibraryStuff[1].Should().BeEquivalentTo([false]);
        stagingLoadData.EnemyEncounter[0].Should().BeEquivalentTo([0, 0]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(EnemyLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsRecipeLibraryEntry_WhenRecipeLibraryEntryIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingRecipeLibraryEntry";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new() { [namedId] = true },
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.LibraryStuff[2].Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(RecipeLibraryEntryLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsRecipeLibraryEntryLocked_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomRecipeLibraryEntry";

        Dictionary<string, RecipeLibraryEntryLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_recipeLibraryEntriesLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.LibraryStuff[2].Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(RecipeLibraryEntryLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsRecord_WhenRecordIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingRecord";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new() { [namedId] = true },
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.LibraryStuff[3].Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(RecordLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsRecordLocked_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomRecord";

        Dictionary<string, RecordLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_recordsLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.LibraryStuff[3].Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(RecordLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsArea_WhenAreaIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingArea";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new() { [namedId] = true },
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.LibraryStuff[4].Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(AreaLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsAreaLocked_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomArea";

        Dictionary<string, AreaLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_areasLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.LibraryStuff[4].Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(AreaLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsFlag_WhenFlagIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingFlag";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new() { [namedId] = true },
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.Flags.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsFlagWithFalse_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomFlag";

        Dictionary<string, FlagLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_flagsLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.Flags.Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsFlagstring_WhenFlagstringIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingFlagstring";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new() { [namedId] = "SomeValue" },
                Flagvars = new(),
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.Flagstrings.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagstringLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsFlagstringWithEmptyString_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomFlagstring";

        Dictionary<string, FlagstringLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_flagstringsLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.Flagstrings.Should().BeEquivalentTo("");

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagstringLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsFlagvar_WhenFlagvarIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingFlagvar";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new() { [namedId] = 5 },
                CrystalBerries = new()
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.Flagvars.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagvarLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsFlagvarWithZero_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomFlagvar";

        Dictionary<string, FlagvarLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_flagvarsLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.Flagvars.Should().BeEquivalentTo([0]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(FlagvarLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public void DeserializeBudsSaveData_SkipsCrystalBerry_WhenCrystalBerryIsNotInRegistry()
    {
        string budId = "bud1";
        string namedId = "MissingCrystalBerry";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId] = new()
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
                CrystalBerries = new() { [namedId] = true }
            }
        };

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        stagingLoadData.CrystalBerryFlags.Should().BeEmpty();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(CrystalBerryLeaf)} named {namedId} created by {budId} does not exists in the registry. " +
            "It will be skipped, but the save will still be loaded.");
    }

    [Fact]
    public void DeserializeBudsSaveData_LoadsCrystalBerryUnobtained_WhenNoBudsDataExistsForIt()
    {
        string budId = "bud1";
        string namedId = "CustomCrystalBerry";

        Dictionary<string, CrystalBerryLeaf> medalShopLeaves =
            new() { [$"{budId}~{namedId}"] = new(0, namedId, budId) };
        MockRegistry(_crystalBerriesLeafRegistry, medalShopLeaves);

        StagingLoadData stagingLoadData = new();

        _sut.DeserializeBudsSaveData(new(), stagingLoadData);

        stagingLoadData.CrystalBerryFlags.Should().BeEquivalentTo([false]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should().Be(
            $"The {nameof(CrystalBerryLeaf)} named {namedId} created by {budId} wasn't found in any of the buds save data. " +
            "It will be loaded with default data.");
    }

    [Fact]
    public Task DeserializeBudsSaveData_AmendsStagingData_WhenBudsSaveDataIsValid()
    {
        string budId1 = "bud1";
        string budId2 = "bud2";
        string budId3 = "bud3";
        string budId4 = "bud4";
        string budId5 = "bud5";

        Dictionary<string, BudSaveData> budsSaveData = new()
        {
            [budId1] = new()
            {
                MedalShops =
                    new()
                    {
                        ["CustomShop"] = new()
                        {
                            AvailablePool = ["bud1~CustomMedal1", "bud2~CustomMedal2"],
                            ShopStock = ["bud2~CustomMedal2", "bud1~CustomMedal1"]
                        }
                    },
                DiscoveryUnlocks = new() { ["CustomDiscovery"] = true },
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            },
            [budId2] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new()
                {
                    ["CustomEnemy"] = new()
                    {
                        IsBestiaryEntryUnlocked = true,
                        AmountSeen = 1,
                        AmountDefeated = 2
                    }
                },
                RecipeLibraryEntryUnlocks = new() { ["CustomRecipeLibraryEntry"] = true },
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            },
            [budId3] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new() { ["CustomRecord"] = true },
                AreaUnlocks = new() { ["CustomArea"] = true },
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new(),
                CrystalBerries = new()
            },
            [budId4] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new() { ["CustomFlag"] = true },
                Flagstrings = new() { ["CustomFlagstring"] = "SomeValue" },
                Flagvars = new(),
                CrystalBerries = new()
            },
            [budId5] = new()
            {
                MedalShops = new(),
                DiscoveryUnlocks = new(),
                Enemies = new(),
                RecipeLibraryEntryUnlocks = new(),
                RecordUnlocks = new(),
                AreaUnlocks = new(),
                Flags = new(),
                Flagstrings = new(),
                Flagvars = new() { ["CustomFlagvar"] = 5 },
                CrystalBerries = new() { ["CustomCrystalBerry"] = true }
            }
        };

        Dictionary<string, MedalShopLeaf> medalShopLeaves = new()
        {
            ["Merab"] = new(0, "Merab", Constants.BaseGameCreatorId),
            [$"{budId1}~CustomShop"] = new(1, "CustomShop", budId1)
        };
        MockRegistry(_medalShopsLeafRegistry, medalShopLeaves);

        Dictionary<string, MedalLeaf> medalLeaves = new()
        {
            [nameof(MainManager.BadgeTypes.HPPlus)] =
                new(0, nameof(MainManager.BadgeTypes.HPPlus), Constants.BaseGameCreatorId),
            [$"{budId2}~CustomMedal2"] = new(1, "CustomMedal2", budId2),
            [$"{budId1}~CustomMedal1"] = new(2, "CustomMedal1", budId1)
        };
        MockRegistry(_medalsLeafRegistry, medalLeaves);

        Dictionary<string, DiscoveryLeaf> discoveryLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId1}~CustomDiscovery"] = new(1, "CustomDiscovery", budId1)
        };
        MockRegistry(_discoveriesLeafRegistry, discoveryLeaves);

        Dictionary<string, EnemyLeaf> enemyLeaves = new()
        {
            [nameof(MainManager.Enemies.CordycepsAnt)] = new(
                0,
                nameof(MainManager.Enemies.CordycepsAnt),
                Constants.BaseGameCreatorId),
            [$"{budId2}~CustomEnemy"] = new(1, "CustomEnemy", budId2)
        };
        MockRegistry(_enemiesLeafRegistry, enemyLeaves);

        Dictionary<string, RecipeLibraryEntryLeaf> recipeLibraryEntryLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId2}~CustomRecipeLibraryEntry"] = new(1, "CustomRecipeLibraryEntry", budId2)
        };
        MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntryLeaves);

        Dictionary<string, RecordLeaf> recordLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId3}~CustomRecord"] = new(1, "CustomRecord", budId3)
        };
        MockRegistry(_recordsLeafRegistry, recordLeaves);

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BugariaOutskirts)] = new(
                0,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId),
            [$"{budId3}~CustomArea"] = new(1, "CustomArea", budId3)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, FlagLeaf> flagLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId4}~CustomFlag"] = new(1, "CustomFlag", budId4)
        };
        MockRegistry(_flagsLeafRegistry, flagLeaves);

        Dictionary<string, FlagstringLeaf> flagstringLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId4}~CustomFlagstring"] = new(1, "CustomFlagstring", budId4)
        };
        MockRegistry(_flagstringsLeafRegistry, flagstringLeaves);

        Dictionary<string, FlagvarLeaf> flagvarLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId5}~CustomFlagvar"] = new(1, "CustomFlagvar", budId5)
        };
        MockRegistry(_flagvarsLeafRegistry, flagvarLeaves);

        Dictionary<string, CrystalBerryLeaf> crystalBerryLeaves = new()
        {
            ["0"] = new(0, "0", Constants.BaseGameCreatorId),
            [$"{budId5}~CustomCrystalBerry"] = new(1, "CustomCrystalBerry", budId5)
        };
        MockRegistry(_crystalBerriesLeafRegistry, crystalBerryLeaves);

        Dictionary<string, string> budsSaveDataByCreatorIds = budsSaveData
            .ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));

        StagingLoadData stagingLoadData = new();
        stagingLoadData.AvaliableBadgePool.Add([]);
        stagingLoadData.BadgeShops.Add([]);
        stagingLoadData.LibraryStuff[0].Add(false);
        stagingLoadData.LibraryStuff[1].Add(false);
        stagingLoadData.LibraryStuff[2].Add(false);
        stagingLoadData.LibraryStuff[3].Add(false);
        stagingLoadData.LibraryStuff[4].Add(false);
        stagingLoadData.Flags.Add(false);
        stagingLoadData.Flagstrings.Add("");
        stagingLoadData.Flagvars.Add(0);
        stagingLoadData.CrystalBerryFlags.Add(false);
        stagingLoadData.EnemyEncounter.Add([0, 0]);

        _sut.DeserializeBudsSaveData(budsSaveDataByCreatorIds, stagingLoadData);

        _logger.Collector.Count.Should().Be(0);
        return Verify(stagingLoadData);
    }

    private static void MockRegistry<TLeaf>(
        ILeavesRegistry<TLeaf> registry,
        Dictionary<string, TLeaf> leavesByEffectiveIds)
        where TLeaf : Leaf
    {
        registry.LeavesByEffectiveIds.Returns(leavesByEffectiveIds);
        registry.LeavesByGameIds.Returns(leavesByEffectiveIds.ToDictionary(x => x.Value.GameId, x => x.Value));
    }
}