namespace VenusRootLoader.BaseGameData;

internal sealed class BaseGameDataCollector
{
    internal static readonly string[] LanguageDisplayNames = MainManager.languagenames.ToArray();

    private readonly BaseGameItemsCollector _baseGameItemsCollector;

    public BaseGameDataCollector(BaseGameItemsCollector baseGameItemsCollector)
    {
        _baseGameItemsCollector = baseGameItemsCollector;
    }

    internal void CollectAndRegisterBaseGameData(string baseGameId)
    {
        _baseGameItemsCollector.CollectAndRegisterItems(baseGameId);
    }
}