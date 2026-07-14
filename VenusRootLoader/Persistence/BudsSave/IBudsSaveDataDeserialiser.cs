namespace VenusRootLoader.Persistence.BudsSave;

internal interface IBudsSaveDataDeserialiser
{
    void DeserialiseBudsSaveData(Dictionary<string, string> budsSaveDataByIds, StagingLoadData stagingLoadData);
}