namespace VenusRootLoader.Persistence.BudsSave;

internal interface IBudsSaveDataSerialiser
{
    Dictionary<string, string> GetBudsSaveDataFromRuntimeState();
}