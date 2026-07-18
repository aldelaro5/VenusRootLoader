using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Text.Json;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BaseGameSave;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Tests.Persistence.BaseGameSave;

public sealed class BaseGameSaveDataDeserializerTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    private readonly string _minimalSaveData =
        File.ReadAllText(Path.Combine("Persistence", "BaseGameSave", "TestFiles", "MinimalValidData.txt"));

    private readonly string _fullSaveData =
        File.ReadAllText(Path.Combine("Persistence", "BaseGameSave", "TestFiles", "FullValidData.txt"));

    private readonly FakeLogger<BaseGameSaveDataDeserializer> _logger = new();
    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsLeafRegistry = Substitute.For<ILeavesRegistry<AnimIdLeaf>>();
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry = Substitute.For<ILeavesRegistry<MapLeaf>>();
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry = Substitute.For<ILeavesRegistry<AreaLeaf>>();
    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry = Substitute.For<ILeavesRegistry<MedalLeaf>>();
    private readonly ILeavesRegistry<QuestLeaf> _questsLeafRegistry = Substitute.For<ILeavesRegistry<QuestLeaf>>();
    private readonly ILeavesRegistry<ItemLeaf> _itemsLeafRegistry = Substitute.For<ILeavesRegistry<ItemLeaf>>();
    private readonly ILeavesRegistry<MusicLeaf> _musicsLeafRegistry = Substitute.For<ILeavesRegistry<MusicLeaf>>();

    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry =
        Substitute.For<ILeavesRegistry<DiscoveryLeaf>>();

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry = Substitute.For<ILeavesRegistry<EnemyLeaf>>();

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

    private readonly BaseGameSaveDataDeserializer _sut;

    public BaseGameSaveDataDeserializerTests()
    {
        _sut = new(
            _logger,
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
    public void DeserializeLiteBaseGameSaveData_ReturnsLiteLoadData_WhenSaveDataIsValid()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,HideoutGarden,BanditHideout,11,12,13,14,15,16,17,3
            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        MainManager.LoadData result = _sut.DeserializeLiteBaseGameSaveData(saveData);

        result.level.Should().Be(5);
        result.areaid.Should().Be((int)MainManager.Areas.BanditHideout);
        result.mapid.Should().Be((int)MainManager.Maps.HideoutGarden);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);
        result.loadpos.Should().Be(new Vector3(4, 5, 6));
        result.challenges.Should().BeEquivalentTo([false, false, false, true, false, false]);
        result.filename.Should().Be("SomeFilename");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DeserializeLiteBaseGameSaveData_ThrowsAnException_WhenSaveDataHasLessThan3Lines(int linesAmount)
    {
        string saveData = string.Join("\n", Enumerable.Repeat("", linesAmount));

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 3 lines in the base game save data");
    }

    [Fact]
    public void DeserializeLiteBaseGameSaveData_ThrowsAnException_WhenHeaderLineHasLessThan10Fields()
    {
        string saveData =
            """
            4,5,6,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,HideoutGarden,BanditHideout,11,12,13,14,15,16,17,3
            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 10 fields in the base game save data header line");
    }

    [Fact]
    public void DeserializeLiteBaseGameSaveData_ThrowsAnException_WhenGeneralInformationLineHasLessThan16Fields()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,9,10,HideoutGarden,BanditHideout,11,12,13,14,15,16,17,3
            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 16 fields in the base game save data general information line");
    }

    [Fact]
    public void DeserializeLiteBaseGameSaveData_ThrowsAnException_WhenMapDoesNotExistInRegistry()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,MissingHideoutGarden,BanditHideout,11,12,13,14,15,16,17,3
            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage(
                $"The save was done at a {nameof(MapLeaf)} named MissingHideoutGarden created by {Constants.BaseGameCreatorId} " +
                $"while no such {nameof(MapLeaf)} exists in the registry.");
    }

    [Fact]
    public void DeserializeLiteBaseGameSaveData_ThrowsAnException_WhenAreaDoesNotExistInRegistry()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,HideoutGarden,MissingBanditHideout,11,12,13,14,15,16,17,3
            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage(
                $"The save was done at an {nameof(AreaLeaf)} named MissingBanditHideout created by {Constants.BaseGameCreatorId} " +
                $"while no such {nameof(AreaLeaf)} exists in the registry.");
    }

    [Fact]
    public void DeserializeFullBaseGameSaveData_ThrowsAnException_WhenSaveDataHasLessThan18Lines()
    {
        string saveData = string.Join("\n", Enumerable.Repeat("", 17));

        StagingLoadData stagingLoadData = new();
        Action action = () => _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 18 lines in the base game save data");
    }

    [Fact]
    public void DeserializeFullBaseGameSaveData_ThrowsAnException_WhenHeaderLineHasLessThan10Fields()
    {
        string saveData =
            """
            4,5,6,False,False,False,False,SomeFilename

            5,6,7,8,9,10,HideoutGarden,BanditHideout,11,12,13,14,15,16,17,3















            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        StagingLoadData stagingLoadData = new();
        Action action = () => _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 10 fields in the base game save data header line");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsPartyMemberAndLogsWarning_WhenPartyMemberAnimIdDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "MissingBeetle";
        saveData[1] = $"Bee,1,2,3,4,5,6,7@{creatorId}~{namedId},8,9,10,11,12,13,14";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.PlayerData.Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The player party member index 1 has an AnimIdLeaf named {namedId} created by {creatorId} while no such " +
                $"{nameof(AnimIdLeaf)} exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void DeserializeFullBaseGameSaveData_ThrowsAnException_WhenGeneralInformationLineHasLessThan16Fields()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,10,HideoutGarden,BanditHideout,11,12,13,14,15,16,17,3















            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        StagingLoadData stagingLoadData = new();
        Action action = () => _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage("There are less than 16 fields in the base game save data general information line");
    }

    [Fact]
    public void DeserializeFullBaseGameSaveData_ThrowsAnException_WhenMapDoesNotExistInRegistry()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,MissingHideoutGarden,BanditHideout,11,12,13,14,15,16,17,3















            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        StagingLoadData stagingLoadData = new();
        Action action = () => _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage(
                $"The save was done at a {nameof(MapLeaf)} named MissingHideoutGarden created by {Constants.BaseGameCreatorId} " +
                $"while no such {nameof(MapLeaf)} exists in the registry.");
    }

    [Fact]
    public void DeserializeFullBaseGameSaveData_ThrowsAnException_WhenAreaDoesNotExistInRegistry()
    {
        string saveData =
            """
            4,5,6,False,False,False,true,False,False,SomeFilename

            5,6,7,8,9,10,HideoutGarden,MissingBanditHideout,11,12,13,14,15,16,17,3















            """;

        Dictionary<string, AreaLeaf> areaLeaves = new()
        {
            [nameof(MainManager.Areas.BanditHideout)] = new(
                (int)MainManager.Areas.BanditHideout,
                nameof(MainManager.Areas.BugariaOutskirts),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [nameof(MainManager.Maps.HideoutGarden)] = new(
                (int)MainManager.Maps.HideoutGarden,
                nameof(MainManager.Maps.HideoutGarden),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Action action = () => _sut.DeserializeLiteBaseGameSaveData(saveData);

        action.Should()
            .Throw<InvalidDataException>()
            .WithMessage(
                $"The save was done at an {nameof(AreaLeaf)} named MissingBanditHideout created by {Constants.BaseGameCreatorId} " +
                $"while no such {nameof(AreaLeaf)} exists in the registry.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsMedalInMedalShopAvailablePoolAndLogsWarning_WhenMedalDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[3] = $"DoublePain,DoublePainReal@BerryFinder,BumpAttack,{creatorId}~{namedId}";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.AvaliableBadgePool[1].Should().HaveCount(2);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The medal shop index 1 medal index 2 is named {namedId} created by {creatorId} " +
                $"while no such MedalLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsMedalInMedalShopStockAndLogsWarning_WhenMedalDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[4] = $"DoublePainReal,DoublePain@BumpAttack,BerryFinder,{creatorId}~{namedId}";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.BadgeShops[1].Should().HaveCount(2);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The medal shop index 1 medal index 2 is named {namedId} created by {creatorId} " +
                $"while no such MedalLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsQuestAndLogsWarning_WhenQuestInOpenBoardDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[5] = $"MenderQuest,{creatorId}~{namedId}@LadybugQuest@ToyQuest";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.BoardQuests[0].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The Open quest board quest index 1 is named {namedId} created by {creatorId} " +
                $"while no such QuestLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsQuestAndLogsWarning_WhenQuestInTakenBoardDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[5] = $"MenderQuest@LadybugQuest,{creatorId}~{namedId}@ToyQuest";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.BoardQuests[1].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The Taken quest board quest index 1 is named {namedId} created by {creatorId} " +
                $"while no such QuestLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsQuestAndLogsWarning_WhenQuestInCompletedBoardDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[5] = $"MenderQuest@LadybugQuest@ToyQuest,{creatorId}~{namedId}";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.BoardQuests[2].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The Completed quest board quest index 1 is named {namedId} created by {creatorId} " +
                $"while no such QuestLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsItemAndLogsWarning_WhenRegularItemDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[6] = $"AntCompass,{creatorId}~{namedId}@BerryShake@ClearWater";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Items[0].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The regular items inventory item index 1 is named {namedId} created by {creatorId} " +
                $"while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsItemAndLogsWarning_WhenKeyItemDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[6] = $"AntCompass@BerryShake,{creatorId}~{namedId}@ClearWater";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Items[1].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The key items inventory item index 1 is named {namedId} created by {creatorId} " +
                $"while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsItemAndLogsWarning_WhenStoredItemDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[6] = $"AntCompass@BerryShake@ClearWater,{creatorId}~{namedId}";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Items[2].Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The stored items inventory item index 1 is named {namedId} created by {creatorId} " +
                $"while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsMedalAndLogsWarning_WhenMedalOnHandDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[7] = $"ChargeUp,Bee@{creatorId}~{namedId},@PoisonAttacker,";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Badges.Should().HaveCount(2);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The medal index 1 is named {namedId} created by {creatorId} while no such MedalLeaf exists in the registry. " +
                $"It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_LeavesMedalUnequippedAndLogsWarning_WhenMedalIsEquippedOnAnAnimIdThatDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[7] = $"ChargeUp,Bee@DefenseExchange,{creatorId}~{namedId}@PoisonAttacker,";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Badges.Should().HaveCount(3);
        stagingLoadData.Badges[1].Should().BeEquivalentTo([50, -2]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The medal index 1 is equipped on someone with an AnimIdLeaf named {namedId} created by {creatorId} while " +
                $"no such AnimIdLeaf exists in the registry. It will be left unequipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsMusicAndLogsWarning_WhenMusicDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[8] = $"Battle0,-1@{creatorId}~{namedId},1";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.SamiraMusics.Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The samira song index 1 is named {namedId} created by {creatorId} while no such " +
                $"MusicLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsStatBonusAndLogsWarning_WhenStatBonusIsOnAnAnimIdThatDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[9] = $"0,1,Beetle@2,3,{creatorId}~{namedId}";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.StatBonus.Should().ContainSingle();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The stat bonus index 1 index is named {namedId} created by {creatorId} while no such AnimIdLeaf " +
                $"exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsItemsAndLogsWarning_WhenRegularItemInChapter4CaptureDataDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        string[] flagstringLine = saveData[12].Split(["|SPLIT|"], StringSplitOptions.None);
        flagstringLine[8] = $"AntCompass,{creatorId}~{namedId},BerryJuice-BerryShake,BadBook-789";
        saveData[12] = string.Join("|SPLIT|", flagstringLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagstrings.Should().HaveElementAt(8, "37,39-173,174-789");

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagstring 8 (Chapter 4 capture data) regular item index 1 is named {namedId} created by {creatorId} " +
                $"while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsItemsAndLogsWarning_WhenKeyItemInChapter4CaptureDataDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        string[] flagstringLine = saveData[12].Split(["|SPLIT|"], StringSplitOptions.None);
        flagstringLine[8] = $"AntCompass,BerryJuice-BerryShake,{creatorId}~{namedId},BadBook-789";
        saveData[12] = string.Join("|SPLIT|", flagstringLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagstrings.Should().HaveElementAt(8, "37,39-173,174-789");

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagstring 8 (Chapter 4 capture data) key item index 1 is named {namedId} created by {creatorId} " +
                $"while no such ItemLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsSpyCardAndLogsWarning_WhenSpyCardInSavedDeckDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        string[] flagstringLine = saveData[12].Split(["|SPLIT|"], StringSplitOptions.None);
        flagstringLine[12] = $"5Card,{creatorId}~{namedId},6Card";
        saveData[12] = string.Join("|SPLIT|", flagstringLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagstrings.Should().HaveElementAt(12, "5,6");

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagstring 12 (saved Spy Cards deck) card index 1 is named {namedId} created by {creatorId} " +
                $"while no such SpyCardLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsMedalAndLogsWarning_WhenMedalInMysteryQueueDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        string[] flagstringLine = saveData[12].Split(["|SPLIT|"], StringSplitOptions.None);
        flagstringLine[13] = $"EXPBoost,{creatorId}~{namedId},ShockTrooper";
        saveData[12] = string.Join("|SPLIT|", flagstringLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagstrings.Should().HaveElementAt(13, "42,34");

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagstring 13 (MYSTERY? medals queue) contains a MedalLeaf named {namedId} created by {creatorId} " +
                $"while no such MedalLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_LoadsFlagvar56AsIsAndLogsWarning_WhenValueIsInteger()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string value = "578";
        string[] flagvarLine = saveData[13].Split(',');
        flagvarLine[56] = value;
        saveData[13] = string.Join(",", flagvarLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagvars.Should().HaveElementAt(56, int.Parse(value));

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagvar 56 (ItemLeaf equipped on Chompy) has a value of {value} which isn't an ItemLeaf that exists in the registry. " +
                $"The flagvar will be loaded as is since it is parsable as an integer and the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_LoadsFlagvar56AsZeroAndLogsWarning_WhenValueIsNotAnIntegerAndItemDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        string[] flagvarLine = saveData[13].Split(',');
        flagvarLine[56] = $"{creatorId}~{namedId}";
        saveData[13] = string.Join(",", flagvarLine);

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.Flagvars.Should().HaveElementAt(56, 0);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The flagvar 56 (ItemLeaf equipped on Chompy) is named {namedId} created by {creatorId} while no such ItemLeaf " +
                $"exists in the registry. The flagvar will be left with a value of 0 since it is not parsable as an integer, " +
                $"but the save file will still be loaded.");
    }

    [Fact]
    public void
        DeserializeFullBaseGameSaveData_SkipsFollowerAndLogsWarning_WhenFollowerAnimIdDoesNotExistInRegistry()
    {
        string[] saveData = _fullSaveData.Split('\n');
        string creatorId = "SomeBud";
        string namedId = "Missing";
        saveData[16] = $"OldMoth,{creatorId}~{namedId},AntQueen";

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();

        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(
            string.Join("\n", saveData),
            stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        stagingLoadData.ExtraFollowers.Should().BeEquivalentTo([210, 97]);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Warning);
        logRecord.Message.Should()
            .Be(
                $"The follower index 1's AnimIdLeaf is named {namedId} created by {creatorId} while no such " +
                $"AnimIdLeaf exists in the registry. It will be skipped, but the save file will still be loaded.");
    }

    [Fact]
    public Task
        DeserializeFullBaseGameSaveData_ReturnsFullLoadDataAndAmendStagingData_WhenSaveDataIsValidWithMinimalData()
    {
        string saveData = _minimalSaveData;

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

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [map.ToString()] = new((int)map, map.ToString(), Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Dictionary<string, DiscoveryLeaf> discoveries = new();
        for (int i = 0; i < discoveriesAmount; i++)
            discoveries.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = discoveriesAmount; i < 10; i++)
            discoveries.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_discoveriesLeafRegistry, discoveries);

        Dictionary<string, EnemyLeaf> enemies = new();
        for (int i = 0; i < enemiesAmount; i++)
            enemies.Add(i.ToString(), new(i, ((MainManager.Enemies)i).ToString(), Constants.BaseGameCreatorId));
        for (int i = enemiesAmount; i < 10; i++)
            enemies.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_enemiesLeafRegistry, enemies);

        Dictionary<string, RecipeLibraryEntryLeaf> recipeLibraryEntries = new();
        for (int i = 0; i < recipeLibraryEntriesAmount; i++)
            recipeLibraryEntries.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = recipeLibraryEntriesAmount; i < 10; i++)
            recipeLibraryEntries.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntries);

        Dictionary<string, RecordLeaf> recordLeaves = new();
        for (int i = 0; i < recordsAmount; i++)
            recordLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = recordsAmount; i < 10; i++)
            recordLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_recordsLeafRegistry, recordLeaves);

        Dictionary<string, AreaLeaf> areaLeaves = new();
        for (int i = 0; i < areasAmount; i++)
        {
            if (i == (int)area)
                continue;
            areaLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        }

        areaLeaves[area.ToString()] = new(1, area.ToString(), Constants.BaseGameCreatorId);

        for (int i = areasAmount; i < 10; i++)
            areaLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));

        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, FlagLeaf> flagLeaves = new();
        for (int i = 0; i < flagsAmount; i++)
            flagLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagsAmount; i < 10; i++)
            flagLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagsLeafRegistry, flagLeaves);

        Dictionary<string, FlagstringLeaf> flagstringLeaves = new();
        for (int i = 0; i < flagstringAmount; i++)
            flagstringLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagstringAmount; i < 10; i++)
            flagstringLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagstringsLeafRegistry, flagstringLeaves);

        Dictionary<string, FlagvarLeaf> flagvarLeaves = new();
        for (int i = 0; i < flagvarAmount; i++)
            flagvarLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagvarAmount; i < 10; i++)
            flagvarLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagvarsLeafRegistry, flagvarLeaves);

        Dictionary<string, CrystalBerryLeaf> crystalBerryLeaves = new();
        for (int i = 0; i < crystalBerriesAmount; i++)
            crystalBerryLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = crystalBerriesAmount; i < 10; i++)
            crystalBerryLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_crystalBerriesLeafRegistry, crystalBerryLeaves);

        StagingLoadData stagingLoadData = new();
        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        result.loadpos.Should().Be(new Vector3(4, 5, 6));
        result.challenges.Should().BeEquivalentTo([false, false, false, false, false, false]);
        result.filename.Should().Be("");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(0);

        _logger.Collector.Count.Should().Be(0);

        return Verify(JsonSerializer.Serialize(stagingLoadData, _jsonSerializerOptions));
    }

    [Fact]
    public Task
        DeserializeFullBaseGameSaveData_ReturnsFullLoadDataAndAmendStagingData_WhenSaveDataIsValidWithFullData()
    {
        string saveData = _fullSaveData;

        MainManager.Areas area = MainManager.Areas.BugariaCity;
        MainManager.Maps map = MainManager.Maps.BugariaMainPlaza;

        FullMockingSetup(map, area);

        StagingLoadData stagingLoadData = new();
        MainManager.LoadData result = _sut.DeserializeFullBaseGameSaveData(saveData, stagingLoadData);

        result.loadpos.Should().Be(new Vector3(1, 2, 3));
        result.challenges.Should().BeEquivalentTo([false, true, true, true, true, true]);
        result.filename.Should().Be("SomeFilename");
        result.level.Should().Be(5);
        result.areaid.Should().Be((int)area);
        result.mapid.Should().Be((int)map);
        result.timeh.Should().Be(15);
        result.timem.Should().Be(16);
        result.times.Should().Be(17);
        result.progression.Should().Be(3);

        _logger.Collector.Count.Should().Be(0);

        return Verify(JsonSerializer.Serialize(stagingLoadData, _jsonSerializerOptions));
    }

    private void FullMockingSetup(MainManager.Maps map, MainManager.Areas area)
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

        MainManager.BadgeTypes medalMystery1 = MainManager.BadgeTypes.EXPBoost;
        MainManager.BadgeTypes medalMystery2 = MainManager.BadgeTypes.ShockTrooper;

        MainManager.Items chompyItem = MainManager.Items.HoneyDrop;

        MainManager.AnimIDs follower1 = MainManager.AnimIDs.OldMoth;
        MainManager.AnimIDs follower2 = MainManager.AnimIDs.AntQueen;

        Dictionary<string, MapLeaf> mapLeaves = new()
        {
            [map.ToString()] = new((int)map, map.ToString(), Constants.BaseGameCreatorId)
        };
        MockRegistry(_mapsLeafRegistry, mapLeaves);

        Dictionary<string, AnimIdLeaf> animIdLeaves = new()
        {
            [nameof(MainManager.AnimIDs.None)] =
                new(-1, nameof(MainManager.AnimIDs.None), Constants.BaseGameCreatorId),
            [nameof(MainManager.AnimIDs.Bee)] =
                new(0, nameof(MainManager.AnimIDs.Bee), Constants.BaseGameCreatorId),
            [nameof(MainManager.AnimIDs.Beetle)] =
                new(1, nameof(MainManager.AnimIDs.Beetle), Constants.BaseGameCreatorId),
            [follower1.ToString()] = new((int)follower1, follower1.ToString(), Constants.BaseGameCreatorId),
            [follower2.ToString()] = new((int)follower2, follower2.ToString(), Constants.BaseGameCreatorId)
        };
        MockRegistry(_animIdsLeafRegistry, animIdLeaves);

        Dictionary<string, MedalLeaf> medalLeaves = new()
        {
            [medalMerab1.ToString()] = new((int)medalMerab1, medalMerab1.ToString(), Constants.BaseGameCreatorId),
            [medalMerab2.ToString()] = new((int)medalMerab2, medalMerab2.ToString(), Constants.BaseGameCreatorId),
            [medalShades1.ToString()] =
                new((int)medalShades1, medalShades1.ToString(), Constants.BaseGameCreatorId),
            [medalShades2.ToString()] =
                new((int)medalShades2, medalShades2.ToString(), Constants.BaseGameCreatorId),
            [medalOnHandEquippedToMember.ToString()] = new(
                (int)medalOnHandEquippedToMember,
                medalOnHandEquippedToMember.ToString(),
                Constants.BaseGameCreatorId),
            [medalOnHandEquippedToParty.ToString()] = new(
                (int)medalOnHandEquippedToParty,
                medalOnHandEquippedToParty.ToString(),
                Constants.BaseGameCreatorId),
            [medalOnHandUnequipped.ToString()] = new(
                (int)medalOnHandUnequipped,
                medalOnHandUnequipped.ToString(),
                Constants.BaseGameCreatorId),
            [medalMystery1.ToString()] = new((int)medalMystery1, medalMystery1.ToString(), Constants.BaseGameCreatorId),
            [medalMystery2.ToString()] = new((int)medalMystery2, medalMystery2.ToString(), Constants.BaseGameCreatorId)
        };
        MockRegistry(_medalsLeafRegistry, medalLeaves);

        Dictionary<string, QuestLeaf> questLeaves = new()
        {
            [openQuest1.ToString()] = new((int)openQuest1, openQuest1.ToString(), Constants.BaseGameCreatorId),
            [openQuest2.ToString()] = new((int)openQuest2, openQuest2.ToString(), Constants.BaseGameCreatorId),
            [takenQuest1.ToString()] = new((int)takenQuest1, takenQuest1.ToString(), Constants.BaseGameCreatorId),
            [takenQuest2.ToString()] = new((int)takenQuest2, takenQuest2.ToString(), Constants.BaseGameCreatorId),
            [completedQuest1.ToString()] =
                new((int)completedQuest1, completedQuest1.ToString(), Constants.BaseGameCreatorId),
            [completedQuest2.ToString()] = new(
                (int)completedQuest2,
                completedQuest2.ToString(),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_questsLeafRegistry, questLeaves);

        Dictionary<string, ItemLeaf> itemLeaves = new()
        {
            [regularItem1.ToString()] =
                new((int)regularItem1, regularItem1.ToString(), Constants.BaseGameCreatorId),
            [regularItem2.ToString()] =
                new((int)regularItem2, regularItem2.ToString(), Constants.BaseGameCreatorId),
            [keyItem1.ToString()] = new((int)keyItem1, keyItem1.ToString(), Constants.BaseGameCreatorId),
            [keyItem2.ToString()] = new((int)keyItem2, keyItem2.ToString(), Constants.BaseGameCreatorId),
            [storedItem1.ToString()] = new((int)storedItem1, storedItem1.ToString(), Constants.BaseGameCreatorId),
            [storedItem2.ToString()] = new((int)storedItem2, storedItem2.ToString(), Constants.BaseGameCreatorId),
            [chompyItem.ToString()] = new((int)chompyItem, chompyItem.ToString(), Constants.BaseGameCreatorId)
        };
        MockRegistry(_itemsLeafRegistry, itemLeaves);

        Dictionary<string, MusicLeaf> musicLeaves = new()
        {
            [samiraSongBought.ToString()] =
                new((int)samiraSongBought, samiraSongBought.ToString(), Constants.BaseGameCreatorId),
            [samiraSongNotBought.ToString()] = new(
                (int)samiraSongNotBought,
                samiraSongNotBought.ToString(),
                Constants.BaseGameCreatorId)
        };
        MockRegistry(_musicsLeafRegistry, musicLeaves);

        Dictionary<string, DiscoveryLeaf> discoveries = new();
        for (int i = 0; i < discoveriesAmount; i++)
            discoveries.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = discoveriesAmount; i < 10; i++)
            discoveries.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_discoveriesLeafRegistry, discoveries);

        Dictionary<string, EnemyLeaf> enemies = new();
        for (int i = 0; i < enemiesAmount; i++)
            enemies.Add(i.ToString(), new(i, ((MainManager.Enemies)i).ToString(), Constants.BaseGameCreatorId));
        for (int i = enemiesAmount; i < 10; i++)
            enemies.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_enemiesLeafRegistry, enemies);

        Dictionary<string, RecipeLibraryEntryLeaf> recipeLibraryEntries = new();
        for (int i = 0; i < recipeLibraryEntriesAmount; i++)
            recipeLibraryEntries.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = recipeLibraryEntriesAmount; i < 10; i++)
            recipeLibraryEntries.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_recipeLibraryEntriesLeafRegistry, recipeLibraryEntries);

        Dictionary<string, RecordLeaf> recordLeaves = new();
        for (int i = 0; i < recordsAmount; i++)
            recordLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = recordsAmount; i < 10; i++)
            recordLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_recordsLeafRegistry, recordLeaves);

        Dictionary<string, SpyCardLeaf> spyCardLeaves = new()
        {
            ["5Card"] = new(5, "5Card", Constants.BaseGameCreatorId),
            ["6Card"] = new(6, "6Card", Constants.BaseGameCreatorId)
        };
        MockRegistry(_spyCardsLeafRegistry, spyCardLeaves);

        Dictionary<string, AreaLeaf> areaLeaves = new();
        for (int i = 0; i < areasAmount; i++)
        {
            if (i == (int)area)
                continue;
            areaLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        }

        areaLeaves[area.ToString()] = new(1, area.ToString(), Constants.BaseGameCreatorId);

        for (int i = areasAmount; i < 10; i++)
            areaLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));

        MockRegistry(_areasLeafRegistry, areaLeaves);

        Dictionary<string, FlagLeaf> flagLeaves = new();
        for (int i = 0; i < flagsAmount; i++)
            flagLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagsAmount; i < 10; i++)
            flagLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagsLeafRegistry, flagLeaves);

        Dictionary<string, FlagstringLeaf> flagstringLeaves = new();
        for (int i = 0; i < flagstringAmount; i++)
            flagstringLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagstringAmount; i < 10; i++)
            flagstringLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagstringsLeafRegistry, flagstringLeaves);

        Dictionary<string, FlagvarLeaf> flagvarLeaves = new();
        for (int i = 0; i < flagvarAmount; i++)
            flagvarLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = flagvarAmount; i < 10; i++)
            flagvarLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_flagvarsLeafRegistry, flagvarLeaves);

        Dictionary<string, CrystalBerryLeaf> crystalBerryLeaves = new();
        for (int i = 0; i < crystalBerriesAmount; i++)
            crystalBerryLeaves.Add(i.ToString(), new(i, i.ToString(), Constants.BaseGameCreatorId));
        for (int i = crystalBerriesAmount; i < 10; i++)
            crystalBerryLeaves.Add(i.ToString(), new(i, i.ToString(), "SomeBud"));
        MockRegistry(_crystalBerriesLeafRegistry, crystalBerryLeaves);
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