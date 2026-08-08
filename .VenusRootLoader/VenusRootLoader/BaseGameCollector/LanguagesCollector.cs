using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class LanguagesCollector : IBaseGameCollector
{
    private static readonly List<string> LanguagesDisplayNames = MainManager.languagenames.ToList();

    private static readonly Dictionary<int, string> LanguagesNamedIds = new()
    {
        [0] = "en-US",
        [1] = "es-419",
        [2] = "pt-BR",
        [3] = "ja-JP",
        [4] = "de-DE",
        [5] = "ko-KR",
        [6] = "ru-RU"
    };

    private readonly ILogger<LanguagesCollector> _logger;
    private readonly ILeavesRegistry<LanguageLeaf> _leavesRegistry;

    public LanguagesCollector(ILogger<LanguagesCollector> logger, ILeavesRegistry<LanguageLeaf> leavesRegistry)
    {
        _logger = logger;
        _leavesRegistry = leavesRegistry;
    }

    public void CollectBaseGameData()
    {
        for (int i = 0; i < LanguagesDisplayNames.Count; i++)
        {
            _leavesRegistry.RegisterExisting(
                i,
                LanguagesNamedIds.TryGetValue(i, out string? languageName) ? languageName : i.ToString());
        }

        _logger.LogInformation(
            "Collected and registered {languageAmount} base game languages",
            LanguagesDisplayNames.Count);
    }
}