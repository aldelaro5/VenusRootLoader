using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using UnityEngine;

namespace VenusRootLoader.Persistence;

internal sealed class SaveDataPersistence : ISaveDataPersistence
{
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SaveDataPersistence> _logger;
    private readonly IBaseGameSaveDataSerialiser _baseGameSaveDataSerialiser;
    private readonly IBudsSaveDataSerialiser _budsSaveDataSerialiser;

    public SaveDataPersistence(
        GameExecutionContext gameExecutionContext,
        IFileSystem fileSystem,
        ILogger<SaveDataPersistence> logger,
        IBaseGameSaveDataSerialiser baseGameSaveDataSerialiser,
        IBudsSaveDataSerialiser budsSaveDataSerialiser)
    {
        _gameExecutionContext = gameExecutionContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _baseGameSaveDataSerialiser = baseGameSaveDataSerialiser;
        _budsSaveDataSerialiser = budsSaveDataSerialiser;
    }

    public bool WriteSaveDataToCurrentSaveSlot(Vector3? playerPositionToSave)
    {
        int saveSlot = MainManager.saveslot;
        string saveSlotDirectory = _fileSystem.Path.Combine(
            _gameExecutionContext.GameDir,
            "SaveData",
            saveSlot.ToString());
        string baseGameSaveFilePath = _fileSystem.Path.Combine(saveSlotDirectory, "BaseGame.dat");

        try
        {
            if (!_fileSystem.Directory.Exists(saveSlotDirectory))
                _fileSystem.Directory.CreateDirectory(saveSlotDirectory);

            string saveData = _baseGameSaveDataSerialiser.GetBaseGameSaveDataFromRuntimeState(playerPositionToSave);
            Dictionary<string, string> budsSaveData = _budsSaveDataSerialiser.GetBudsSaveDataFromRuntimeState();

            _fileSystem.File.WriteAllText(baseGameSaveFilePath, saveData);
            foreach (KeyValuePair<string, string> budSaveData in budsSaveData)
            {
                string budSaveDataFilePath = _fileSystem.Path.Combine(saveSlotDirectory, $"{budSaveData.Key}.json");
                _fileSystem.File.WriteAllText(budSaveDataFilePath, budSaveData.Value);
            }

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