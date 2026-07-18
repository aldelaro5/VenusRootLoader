using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using UnityEngine;
using VenusRootLoader.Api;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BaseGameSave;
using VenusRootLoader.Persistence.BudsSave;

namespace VenusRootLoader.Tests.Persistence;

public sealed class SaveDataPersistenceTests
{
    private static readonly string RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "C:\\";
    private static readonly string BudsPath = Path.Combine(RootPath, "Buds");
    private static readonly string SaveDataPath = Path.Combine(RootPath, "SaveData");
    private static readonly string ConfigPath = Path.Combine(RootPath, "Config");
    private static readonly string LoaderPath = Path.Combine(RootPath, nameof(VenusRootLoader));

    private readonly BudLoaderContext _budLoaderContext = new()
    {
        BudsPath = BudsPath,
        SaveDataPath = SaveDataPath,
        ConfigPath = ConfigPath,
        LoaderPath = LoaderPath
    };

    private readonly IGameDataRuntimeState _gameDataRuntimeState = Substitute.For<IGameDataRuntimeState>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly FakeLogger<SaveDataPersistence> _logger = new();

    private readonly IBaseGameSaveDataDeserializer _baseGameSaveDataDeserializer =
        Substitute.For<IBaseGameSaveDataDeserializer>();

    private readonly IBudsSaveDataDeserializer _budsSaveDataDeserializer = Substitute.For<IBudsSaveDataDeserializer>();

    private readonly IBaseGameSaveDataSerializer _baseGameSaveDataSerializer =
        Substitute.For<IBaseGameSaveDataSerializer>();

    private readonly IBudsSaveDataSerializer _budsSaveDataSerializer = Substitute.For<IBudsSaveDataSerializer>();

    private readonly SaveDataPersistence _sut;

    public SaveDataPersistenceTests()
    {
        _sut = new(
            _budLoaderContext,
            _gameDataRuntimeState,
            _fileSystem,
            _logger,
            _baseGameSaveDataDeserializer,
            _budsSaveDataDeserializer,
            _baseGameSaveDataSerializer,
            _budsSaveDataSerializer);
    }

    [Fact]
    public void SaveSlotExistsInVenusRootLoader_ReturnsTrue_WhenSaveSlotExists()
    {
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat"), new(""));

        bool result = _sut.SaveSlotExistsInVenusRootLoader(0);

        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void SaveSlotExistsInVenusRootLoader_ReturnsFalse_WhenSaveSlotDoesNotExists()
    {
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat"), new(""));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "1Backup", "BaseGame.dat"), new(""));

        bool result = _sut.SaveSlotExistsInVenusRootLoader(1);

        result.Should().BeFalse();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void LoadLiteSaveDataFromSlot_ReturnsLoadData_WhenLoadingSucceeds()
    {
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat"), new("BaseGameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "1", "BaseGame.dat"), new("BadData"));

        MainManager.LoadData loadData = new()
        {
            loadpos = new(1, 2, 3),
            mapid = 4,
            areaid = 5,
            level = 6,
            times = 7,
            timem = 8,
            timeh = 9,
            progression = 10,
            filename = "SomeFilename",
            challenges =
            [
                false,
                true
            ]
        };

        _baseGameSaveDataDeserializer.DeserializeLiteBaseGameSaveData(Arg.Any<string>())
            .Returns(loadData);

        MainManager.LoadData? result = _sut.LoadLiteSaveDataFromSlot(0);

        _baseGameSaveDataDeserializer.Received(1).DeserializeLiteBaseGameSaveData("BaseGameData");
        _baseGameSaveDataDeserializer.DidNotReceive().DeserializeLiteBaseGameSaveData("BadData");

        result.Should().Be(loadData);

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void LoadLiteSaveDataFromSlot_ReturnsNull_WhenLoadingFails()
    {
        string directory = Path.Combine(SaveDataPath, "0");
        _fileSystem.AddFile(Path.Combine(directory, "BaseGame.dat"), new("BaseGameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "1", "BaseGame.dat"), new("BadData"));

        Exception exception = new("TestException");
        _baseGameSaveDataDeserializer.DeserializeLiteBaseGameSaveData(Arg.Any<string>())
            .Throws(exception);

        MainManager.LoadData? result = _sut.LoadLiteSaveDataFromSlot(0);

        _baseGameSaveDataDeserializer.Received(1).DeserializeLiteBaseGameSaveData("BaseGameData");
        _baseGameSaveDataDeserializer.DidNotReceive().DeserializeLiteBaseGameSaveData("BadData");

        result.Should().BeNull();

        _logger.Collector.Count.Should().Be(1);

        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Exception.Should().Be(exception);
        logRecord.Message.Should().Be(
            $"An error occured while loading lite save data from {directory}, " +
            $"this save will not be loadable in the game.\n");
    }

    [Fact]
    public void LoadFullSaveDataFromSlot_ReturnsLoadData_WhenLoadingSucceedsWithoutBudsData()
    {
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat"), new("BaseGameData"));

        MainManager.LoadData loadData = new()
        {
            loadpos = new(1, 2, 3),
            mapid = 4,
            areaid = 5,
            level = 6,
            times = 7,
            timem = 8,
            timeh = 9,
            progression = 10,
            filename = "SomeFilename",
            challenges =
            [
                false,
                true
            ]
        };

        _baseGameSaveDataDeserializer.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>())
            .Returns(loadData);
        _baseGameSaveDataDeserializer.When(x =>
                x.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>()))
            .Do(x => x.Arg<StagingLoadData>()!.PartyLevel = 5);

        MainManager.LoadData? result = _sut.LoadFullSaveDataFromSlot(0);

        result.Should().Be(loadData);

        _baseGameSaveDataDeserializer.Received(1)
            .DeserializeFullBaseGameSaveData("BaseGameData", Arg.Any<StagingLoadData>());
        _budsSaveDataDeserializer.Received(1)
            .DeserializeBudsSaveData(
                Arg.Is<Dictionary<string, string>>(x => x!.Count == 0),
                Arg.Any<StagingLoadData>());

        _gameDataRuntimeState.Received(1).PartyLevel = 5;

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void LoadFullSaveDataFromSlot_ReturnsLoadData_WhenLoadingSucceedsWithBudsData()
    {
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat"), new("BaseGameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "Bud1.json"), new("Bud1GameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "Bud2.json"), new("Bud2GameData"));

        MainManager.LoadData loadData = new()
        {
            loadpos = new(1, 2, 3),
            mapid = 4,
            areaid = 5,
            level = 6,
            times = 7,
            timem = 8,
            timeh = 9,
            progression = 10,
            filename = "SomeFilename",
            challenges =
            [
                false,
                true
            ]
        };

        _baseGameSaveDataDeserializer.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>())
            .Returns(loadData);
        _baseGameSaveDataDeserializer.When(x =>
                x.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>()))
            .Do(x => x.Arg<StagingLoadData>()!.PartyLevel = 5);
        _budsSaveDataDeserializer.When(x =>
                x.DeserializeBudsSaveData(Arg.Any<Dictionary<string, string>>(), Arg.Any<StagingLoadData>()))
            .Do(x => x.Arg<StagingLoadData>()!.PartyExp = 5);

        MainManager.LoadData? result = _sut.LoadFullSaveDataFromSlot(0);

        result.Should().Be(loadData);

        _baseGameSaveDataDeserializer.Received(1)
            .DeserializeFullBaseGameSaveData("BaseGameData", Arg.Any<StagingLoadData>());
        _budsSaveDataDeserializer.Received(1)
            .DeserializeBudsSaveData(
                Arg.Is<Dictionary<string, string>>(x => x!.Count == 2 &&
                                                        x["Bud1"] == "Bud1GameData" &&
                                                        x["Bud2"] == "Bud2GameData"),
                Arg.Any<StagingLoadData>());

        _gameDataRuntimeState.Received(1).PartyLevel = 5;
        _gameDataRuntimeState.Received(1).PartyExp = 5;

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void LoadFullSaveDataFromSlot_ReturnsNull_WhenBaseGameLoadingFails()
    {
        string directory = Path.Combine(SaveDataPath, "0");
        _fileSystem.AddFile(Path.Combine(directory, "BaseGame.dat"), new("BaseGameData"));

        Exception exception = new("TestException");
        _baseGameSaveDataDeserializer.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>())
            .Throws(exception);

        MainManager.LoadData? result = _sut.LoadFullSaveDataFromSlot(0);

        result.Should().BeNull();

        _baseGameSaveDataDeserializer.Received(1)
            .DeserializeFullBaseGameSaveData("BaseGameData", Arg.Any<StagingLoadData>());
        _budsSaveDataDeserializer.DidNotReceiveWithAnyArgs()
            .DeserializeBudsSaveData(null!, null!);

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Exception.Should().Be(exception);
        logRecord.Message.Should().Be(
            $"An error occured while loading full save data from {directory}, the game will softlock.\n");
    }

    [Fact]
    public void LoadFullSaveDataFromSlot_ReturnsNull_WhenBudsGameDataLoadingFails()
    {
        string directory = Path.Combine(SaveDataPath, "0");
        _fileSystem.AddFile(Path.Combine(directory, "BaseGame.dat"), new("BaseGameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "Bud1.json"), new("Bud1GameData"));
        _fileSystem.AddFile(Path.Combine(SaveDataPath, "0", "Bud2.json"), new("Bud2GameData"));

        MainManager.LoadData loadData = new()
        {
            loadpos = new(1, 2, 3),
            mapid = 4,
            areaid = 5,
            level = 6,
            times = 7,
            timem = 8,
            timeh = 9,
            progression = 10,
            filename = "SomeFilename",
            challenges =
            [
                false,
                true
            ]
        };

        _baseGameSaveDataDeserializer.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>())
            .Returns(loadData);
        _baseGameSaveDataDeserializer.When(x =>
                x.DeserializeFullBaseGameSaveData(Arg.Any<string>(), Arg.Any<StagingLoadData>()))
            .Do(x => x.Arg<StagingLoadData>()!.PartyLevel = 5);

        Exception exception = new("TestException");
        _budsSaveDataDeserializer.When(x => x.DeserializeBudsSaveData(
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<StagingLoadData>()))
            .Throws(exception);

        MainManager.LoadData? result = _sut.LoadFullSaveDataFromSlot(0);

        result.Should().BeNull();

        _baseGameSaveDataDeserializer.Received(1)
            .DeserializeFullBaseGameSaveData("BaseGameData", Arg.Any<StagingLoadData>());
        _budsSaveDataDeserializer.Received(1)
            .DeserializeBudsSaveData(
                Arg.Is<Dictionary<string, string>>(x => x!.Count == 2 &&
                                                        x["Bud1"] == "Bud1GameData" &&
                                                        x["Bud2"] == "Bud2GameData"),
                Arg.Any<StagingLoadData>());

        _gameDataRuntimeState.DidNotReceiveWithAnyArgs().PartyLevel = 5;

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Exception.Should().Be(exception);
        logRecord.Message.Should().Be(
            $"An error occured while loading full save data from {directory}, the game will softlock.\n");
    }

    [Fact]
    public void WriteSaveDataToCurrentSaveSlot_ReturnsTrueAndWritesSaveData_WhenNoBudsExists()
    {
        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Returns("BaseGameData");
        _budsSaveDataSerializer.GetBudsSaveDataFromRuntimeState()
            .Returns(new Dictionary<string, string>());

        bool result = _sut.WriteSaveDataToSaveSlot(0, null);

        result.Should().BeTrue();

        _baseGameSaveDataSerializer.Received(1).GetBaseGameSaveDataFromRuntimeState(null);
        _budsSaveDataSerializer.Received(1).GetBudsSaveDataFromRuntimeState();

        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat")).TextContents
            .Should().Be("BaseGameData");
        _fileSystem.FileExists(Path.Combine(SaveDataPath, "0Backup", "BaseGame.dat"))
            .Should().BeFalse();

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void WriteSaveDataToCurrentSaveSlot_ReturnsTrueAndWritesSaveData_WhenBudsExists()
    {
        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Returns("BaseGameData");
        _budsSaveDataSerializer.GetBudsSaveDataFromRuntimeState()
            .Returns(
                new Dictionary<string, string>
                {
                    ["Bud1"] = "Bud1GameData",
                    ["Bud2"] = "Bud2GameData"
                });

        bool result = _sut.WriteSaveDataToSaveSlot(0, new(1, 2, 3));

        result.Should().BeTrue();

        _baseGameSaveDataSerializer.Received(1).GetBaseGameSaveDataFromRuntimeState(new(1, 2, 3));
        _budsSaveDataSerializer.Received(1).GetBudsSaveDataFromRuntimeState();

        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat")).TextContents
            .Should().Be("BaseGameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "Bud1.json")).TextContents
            .Should().Be("Bud1GameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "Bud2.json")).TextContents
            .Should().Be("Bud2GameData");
        _fileSystem.FileExists(Path.Combine(SaveDataPath, "0Backup", "BaseGame.dat"))
            .Should().BeFalse();

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void WriteSaveDataToCurrentSaveSlot_ReturnsTrueAndMoveExistingToBackup_WhenExistingSaveExists()
    {
        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Returns("OldGameData");
        _budsSaveDataSerializer.GetBudsSaveDataFromRuntimeState()
            .Returns(
                new Dictionary<string, string>
                {
                    ["Bud1"] = "OldBud1GameData",
                    ["Bud2"] = "OldBud2GameData"
                });

        _sut.WriteSaveDataToSaveSlot(0, null);

        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Returns("BaseGameData");
        _budsSaveDataSerializer.GetBudsSaveDataFromRuntimeState()
            .Returns(
                new Dictionary<string, string>
                {
                    ["Bud1"] = "Bud1GameData",
                    ["Bud2"] = "Bud2GameData"
                });

        _sut.WriteSaveDataToSaveSlot(0, null);

        _baseGameSaveDataSerializer.Received(2).GetBaseGameSaveDataFromRuntimeState(null);
        _budsSaveDataSerializer.Received(2).GetBudsSaveDataFromRuntimeState();

        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "BaseGame.dat")).TextContents
            .Should().Be("BaseGameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "Bud1.json")).TextContents
            .Should().Be("Bud1GameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0", "Bud2.json")).TextContents
            .Should().Be("Bud2GameData");

        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0backup", "BaseGame.dat")).TextContents
            .Should().Be("OldGameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0backup", "Bud1.json")).TextContents
            .Should().Be("OldBud1GameData");
        _fileSystem.GetFile(Path.Combine(SaveDataPath, "0backup", "Bud2.json")).TextContents
            .Should().Be("OldBud2GameData");

        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void WriteSaveDataToCurrentSaveSlot_ReturnsFalse_WhenBaseGameSavingFails()
    {
        string directory = Path.Combine(SaveDataPath, "0");
        Exception exception = new("Test");

        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Throws(exception);

        bool result = _sut.WriteSaveDataToSaveSlot(0, null);

        result.Should().BeFalse();

        _baseGameSaveDataSerializer.Received(1).GetBaseGameSaveDataFromRuntimeState(null);
        _budsSaveDataSerializer.DidNotReceiveWithAnyArgs().GetBudsSaveDataFromRuntimeState();

        _fileSystem.FileExists(Path.Combine(directory, "BaseGame.dat"))
            .Should().BeFalse();
        _fileSystem.FileExists(Path.Combine(SaveDataPath, "0Backup", "BaseGame.dat"))
            .Should().BeFalse();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Exception.Should().Be(exception);
        logRecord.Message.Should().Be($"An error occured while writing save data to {directory}\n");
    }

    [Fact]
    public void WriteSaveDataToCurrentSaveSlot_ReturnsFalse_WhenBudsSavingFails()
    {
        string directory = Path.Combine(SaveDataPath, "0");
        Exception exception = new("Test");

        _baseGameSaveDataSerializer.GetBaseGameSaveDataFromRuntimeState(Arg.Any<Vector3?>())
            .Returns("BaseGameData");
        _budsSaveDataSerializer.GetBudsSaveDataFromRuntimeState()
            .Throws(exception);

        bool result = _sut.WriteSaveDataToSaveSlot(0, null);

        result.Should().BeFalse();

        _baseGameSaveDataSerializer.Received(1).GetBaseGameSaveDataFromRuntimeState(null);
        _budsSaveDataSerializer.Received(1).GetBudsSaveDataFromRuntimeState();

        _fileSystem.FileExists(Path.Combine(directory, "BaseGame.dat"))
            .Should().BeFalse();
        _fileSystem.FileExists(Path.Combine(SaveDataPath, "0Backup", "BaseGame.dat"))
            .Should().BeFalse();

        _logger.Collector.Count.Should().Be(1);
        FakeLogRecord logRecord = _logger.LatestRecord;
        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Exception.Should().Be(exception);
        logRecord.Message.Should().Be($"An error occured while writing save data to {directory}\n");
    }
}