namespace VenusRootLoader.Persistence;

internal interface IBudsSaveDataDeserialiser
{
    void DeserialiseBudsSaveData(Dictionary<string, string> budsSaveDataByIds, StagingLoadData stagingLoadData);
}