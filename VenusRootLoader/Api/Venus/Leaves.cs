using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId)
    {
        ItemLeaf leaf = _registryResolver.Resolve<ItemLeaf>().RegisterNew(namedId, _budId);
        LogRegisterContent("Item", namedId, leaf);
        return leaf;
    }

    public ItemLeaf GetItem(string namedId) => _registryResolver.Resolve<ItemLeaf>().Get(namedId);
    public IReadOnlyCollection<ItemLeaf> GetAllItems() => _registryResolver.Resolve<ItemLeaf>().GetAll();

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