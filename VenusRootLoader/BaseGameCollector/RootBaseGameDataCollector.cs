namespace VenusRootLoader.BaseGameCollector;

internal sealed class RootBaseGameDataCollector
{
    internal static readonly string[] LanguageDisplayNames = MainManager.languagenames.ToArray();

    private readonly IEnumerable<IBaseGameCollector> _baseGameCollectors;

    public RootBaseGameDataCollector(IEnumerable<IBaseGameCollector> baseGameCollectors) =>
        _baseGameCollectors = baseGameCollectors;

    internal void CollectAndRegisterBaseGameData(string baseGameId)
    {
        foreach (IBaseGameCollector baseGameCollector in _baseGameCollectors)
            baseGameCollector.CollectBaseGameData(baseGameId);
    }
}