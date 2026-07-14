using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using UnityEngine;
using VenusRootLoader.Api;
using VenusRootLoader.Persistence.BaseGameSave;
using VenusRootLoader.Persistence.BudsSave;

namespace VenusRootLoader.Persistence;

internal sealed class SaveDataPersistence : ISaveDataPersistence
{
    private readonly BudLoaderContext _budLoaderContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SaveDataPersistence> _logger;
    private readonly IBaseGameSaveDataDeserialiser _baseGameSaveDataDeserialiser;
    private readonly IBudsSaveDataDeserialiser _budsSaveDataDeserialiser;
    private readonly IBaseGameSaveDataSerialiser _baseGameSaveDataSerialiser;
    private readonly IBudsSaveDataSerialiser _budsSaveDataSerialiser;

    public SaveDataPersistence(
        BudLoaderContext budLoaderContext,
        IFileSystem fileSystem,
        ILogger<SaveDataPersistence> logger,
        IBaseGameSaveDataDeserialiser baseGameSaveDataDeserialiser,
        IBudsSaveDataDeserialiser budsSaveDataDeserialiser,
        IBaseGameSaveDataSerialiser baseGameSaveDataSerialiser,
        IBudsSaveDataSerialiser budsSaveDataSerialiser)
    {
        _budLoaderContext = budLoaderContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _baseGameSaveDataSerialiser = baseGameSaveDataSerialiser;
        _budsSaveDataSerialiser = budsSaveDataSerialiser;
        _budsSaveDataDeserialiser = budsSaveDataDeserialiser;
        _baseGameSaveDataDeserialiser = baseGameSaveDataDeserialiser;
    }

    public bool SaveSlotExistsInVenusRootLoader(int saveSlot)
    {
        string saveSlotDirectory = _fileSystem.Path.Combine(_budLoaderContext.SaveDataPath, saveSlot.ToString());
        string baseGameSaveFilePath = _fileSystem.Path.Combine(saveSlotDirectory, "BaseGame.dat");
        return _fileSystem.File.Exists(baseGameSaveFilePath);
    }

    public MainManager.LoadData? LoadFullSaveDataFromSlot(int saveSlot)
    {
        string saveSlotDirectory = _fileSystem.Path.Combine(_budLoaderContext.SaveDataPath, saveSlot.ToString());
        string baseGameSaveFilePath = _fileSystem.Path.Combine(saveSlotDirectory, "BaseGame.dat");

        try
        {
            string baseGameSaveData = _fileSystem.File.ReadAllText(baseGameSaveFilePath);
            
            StagingLoadData stagingLoadData = new();
            MainManager.LoadData? loadData =
                _baseGameSaveDataDeserialiser.DeserialiseFullBaseGameSaveData(baseGameSaveData, stagingLoadData);

            Dictionary<string, string> budsSaveDataByIds = new();
            foreach (string budSaveFilePath in _fileSystem.Directory.EnumerateFiles(saveSlotDirectory, "*.json"))
            {
                string budId = _fileSystem.Path.GetFileNameWithoutExtension(budSaveFilePath);
                string budSaveData = _fileSystem.File.ReadAllText(budSaveFilePath);
                budsSaveDataByIds.Add(budId, budSaveData);
            }

            _budsSaveDataDeserialiser.DeserialiseBudsSaveData(budsSaveDataByIds, stagingLoadData);
            
            stagingLoadData.CommitToRuntimeState();
            return loadData;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An error occured while loading full save data from {path}, " +
                "this save will not be loadable in the game: {error}",
                saveSlotDirectory,
                e.ToString());
            return null;
        }
    }

    public MainManager.LoadData? LoadLiteSaveDataFromSlot(int saveSlot)
    {
        string saveSlotDirectory = _fileSystem.Path.Combine(_budLoaderContext.SaveDataPath, saveSlot.ToString());
        string baseGameSaveFilePath = _fileSystem.Path.Combine(saveSlotDirectory, "BaseGame.dat");

        try
        {
            string baseGameSaveData = _fileSystem.File.ReadAllText(baseGameSaveFilePath);
            return _baseGameSaveDataDeserialiser.DeserialiseLiteBaseGameSaveData(baseGameSaveData);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An error occured while loading lite save data from {path}, " +
                "this save will not be loadable in the game: {error}",
                saveSlotDirectory,
                e.ToString());
            return null;
        }
    }

    public bool WriteSaveDataToCurrentSaveSlot(Vector3? playerPositionToSave)
    {
        int saveSlot = MainManager.saveslot;
        string saveSlotDirectory = _fileSystem.Path.Combine(_budLoaderContext.SaveDataPath, saveSlot.ToString());
        string temporarySaveSlotDirectory = saveSlotDirectory + "temp";
        string backupSaveSlotDirectory = saveSlotDirectory + "backup";
        string baseGameSaveFilePath = _fileSystem.Path.Combine(temporarySaveSlotDirectory, "BaseGame.dat");

        try
        {
            string saveData = _baseGameSaveDataSerialiser.GetBaseGameSaveDataFromRuntimeState(playerPositionToSave);
            Dictionary<string, string> budsSaveData = _budsSaveDataSerialiser.GetBudsSaveDataFromRuntimeState();

            if (_fileSystem.Directory.Exists(temporarySaveSlotDirectory))
                _fileSystem.Directory.Delete(temporarySaveSlotDirectory);
            _fileSystem.Directory.CreateDirectory(temporarySaveSlotDirectory);

            _fileSystem.File.WriteAllText(baseGameSaveFilePath, saveData);
            foreach (KeyValuePair<string, string> budSaveData in budsSaveData)
            {
                string budSaveDataFilePath = _fileSystem.Path.Combine(
                    temporarySaveSlotDirectory,
                    $"{budSaveData.Key}.json");
                _fileSystem.File.WriteAllText(budSaveDataFilePath, budSaveData.Value);
            }

            if (_fileSystem.Directory.Exists(saveSlotDirectory))
            {
                if (_fileSystem.Directory.Exists(backupSaveSlotDirectory))
                    _fileSystem.Directory.Delete(backupSaveSlotDirectory, true);

                _fileSystem.Directory.Move(saveSlotDirectory, backupSaveSlotDirectory);
            }

            _fileSystem.Directory.Move(temporarySaveSlotDirectory, saveSlotDirectory);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An error occured while writing save data to {path}: {error}",
                saveSlotDirectory,
                e.ToString());
            return false;
        }
    }
}