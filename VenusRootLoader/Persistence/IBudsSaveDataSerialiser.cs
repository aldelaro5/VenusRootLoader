namespace VenusRootLoader.Persistence;

internal interface IBudsSaveDataSerialiser
{
    Dictionary<string, string> GetBudsSaveDataFromRuntimeState();
}