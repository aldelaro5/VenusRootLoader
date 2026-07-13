namespace VenusRootLoader.Persistence;

internal interface IBaseGameSaveDataDeserialiser
{
    MainManager.LoadData DeserialiseLiteBaseGameSaveData(string saveData);
    MainManager.LoadData DeserialiseFullBaseGameSaveData(string saveData, StagingLoadData stagingLoadData);
}