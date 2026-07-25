namespace VenusRootLoader.Persistence.BudsSave;

internal interface IBudsSaveDataSerializer
{
    Dictionary<string, string> GetBudsSaveDataFromRuntimeState();
}