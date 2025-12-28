using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace VenusRootLoader.Public;

public abstract class Leaf<TGameId>
{
    public abstract TGameId GameId { get; }
    public abstract string NamedId { get; }
    public abstract string CreatorId { get; }
    public required string OwnerId { get; init; }

    private readonly ILogger<Venus> _logger;
    protected abstract string ContentTypeName { get; }

    protected Leaf(ILogger<Venus> logger) => _logger = logger;

    internal void LogChange([CallerMemberName] string name = "")
    {
        if (OwnerId == CreatorId)
            return;

        _logger.LogTrace(
            "{OwnerId} changed the {ContentTypeName} named {NameId} (game id {GameId}) that was created by {CreatorId} using {ChangeName}",
            OwnerId,
            ContentTypeName,
            NamedId,
            GameId,
            CreatorId,
            name);
    }

    protected T GetOrCreateLanguageDataForLanguage<T>(int languageId, Dictionary<int, T> languageData)
        where T : new()
    {
        if (languageData.TryGetValue(languageId, out T? itemLanguageData))
            return itemLanguageData;

        itemLanguageData = new();
        languageData[languageId] = itemLanguageData;
        return itemLanguageData;
    }
}