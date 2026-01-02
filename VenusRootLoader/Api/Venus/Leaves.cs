using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId)
    {
        ItemLeaf leaf = _leavesRegistry.RegisterNew(namedId, _budId);
        LogRegisterContent("Item", namedId, leaf);
        return leaf;
    }

    public ItemLeaf RequestItem(string namedId) => _leavesRegistry.Get(namedId);

    private void LogRegisterContent(string contentType, string namedId, ItemLeaf leaf)
    {
        _logger.LogTrace(
            "{BudId} registered a new {ContentType} named {NamedId} (game id {GameId})",
            _budId,
            contentType,
            namedId,
            leaf.GameId);
    }
}