using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.GameContent;
using VenusRootLoader.Public.Leaves;

// ReSharper disable CheckNamespace

namespace VenusRootLoader.Public;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId)
    {
        Guard.IsNotNullOrWhiteSpace(namedId);
        ItemContent content = _contentRegistry.RegisterAndBindNewItem(namedId, _budId);
        LogRegisterContent("Item", namedId, content);
        return new(content, _logger) { OwnerId = _budId };
    }

    public ItemLeaf RequestItem(string namedId)
    {
        Guard.IsNotNullOrWhiteSpace(namedId);
        return new(_contentRegistry.RequestExistingItem(namedId), _logger) { OwnerId = _budId };
    }

    private void LogRegisterContent(string contentType, string namedId, ItemContent content)
    {
        _logger.LogTrace(
            "{BudId} registered a new {ContentType} named {NamedId} (game id {GameId})",
            _budId,
            contentType,
            namedId,
            content.GameId);
    }
}