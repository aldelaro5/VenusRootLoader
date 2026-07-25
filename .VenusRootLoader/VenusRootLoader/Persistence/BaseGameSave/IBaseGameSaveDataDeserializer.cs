namespace VenusRootLoader.Persistence.BaseGameSave;

internal interface IBaseGameSaveDataDeserializer
{
    MainManager.LoadData DeserializeLiteBaseGameSaveData(string saveData);
    MainManager.LoadData DeserializeFullBaseGameSaveData(string saveData, StagingLoadData stagingLoadData);
}