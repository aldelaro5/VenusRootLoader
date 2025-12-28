using Microsoft.Extensions.Logging;
using VenusRootLoader.GameContent;
using VenusRootLoader.Public.Leaves;

// ReSharper disable CheckNamespace

namespace VenusRootLoader.Public;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId)
    {
        EnsureNamedIdIsFree("Item", namedId, _venusServices.GlobalContentRegistry.Items);
        ItemContent itemContent = _venusServices.ContentBinder.Items.BindNew(namedId, _budId);
        _venusServices.GlobalContentRegistry.Items[namedId] = itemContent;
        LogRegisterContent("Item", namedId, itemContent.GameId);
        return new ItemLeaf(itemContent, _venusServices.Logger) { OwnerId = _budId };
    }

    public ItemLeaf RequestItem(string namedId)
    {
        ItemContent content = EnsureNamedIdExists("Item", namedId, _venusServices.GlobalContentRegistry.Items);
        return new ItemLeaf(content, _venusServices.Logger) { OwnerId = _budId };
    }

    private void LogRegisterContent<T>(string contentType, string namedId, T gameId)
    {
        _venusServices.Logger.LogTrace(
            "{BudId} registered a new {ContentType} named {NamedId} (game id {GameId})",
            _budId,
            contentType,
            namedId,
            gameId);
    }

    private static void EnsureNamedIdIsFree<T>(string contentType, string namedId, Dictionary<string, T> registry)
    {
        if (registry.ContainsKey(namedId))
            throw new Exception($"{contentType} with namedId {namedId} already exists");
    }

    private static T EnsureNamedIdExists<T>(string contentType, string namedId, Dictionary<string, T> registry)
    {
        return !registry.TryGetValue(namedId, out T content)
            ? throw new Exception($"{contentType} with namedId {namedId} does not exist")
            : content;
    }
}