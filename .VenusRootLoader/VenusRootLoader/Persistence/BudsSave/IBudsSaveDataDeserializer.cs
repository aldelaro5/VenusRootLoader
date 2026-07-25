namespace VenusRootLoader.Persistence.BudsSave;

internal interface IBudsSaveDataDeserializer
{
    void DeserializeBudsSaveData(Dictionary<string, string> budsSaveDataByIds, StagingLoadData stagingLoadData);
}