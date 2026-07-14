namespace VenusRootLoader.Persistence.BaseGameSave;

internal interface IBaseGameSaveDataDeserialiser
{
    MainManager.LoadData DeserialiseLiteBaseGameSaveData(string saveData);
    MainManager.LoadData DeserialiseFullBaseGameSaveData(string saveData, StagingLoadData stagingLoadData);
}