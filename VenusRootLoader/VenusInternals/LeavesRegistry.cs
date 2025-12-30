using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeafBinding;

namespace VenusRootLoader.VenusInternals;

internal sealed class LeavesRegistry
{
    private readonly ILeafBinder<ItemLeaf, int> _itemLeafBinder;

    private Dictionary<string, ItemLeaf> Items { get; } = new();

    public LeavesRegistry(ILeafBinder<ItemLeaf, int> itemLeafBinder)
    {
        _itemLeafBinder = itemLeafBinder;
    }

    internal ItemLeaf RegisterAndBindNewItem(string namedId, string creatorId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        EnsureNamedIdIsFree(namedId, Items);
        ItemLeaf itemLeaf = _itemLeafBinder.BindNew(namedId, creatorId);
        Items[namedId] = itemLeaf;
        return itemLeaf;
    }

    internal ItemLeaf RegisterAndBindExistingItem(int gameId, string namedId, string creatorId)
    {
        ItemLeaf itemLeaf = _itemLeafBinder.BindExisting(gameId, namedId, creatorId);
        Items[namedId] = itemLeaf;
        return itemLeaf;
    }

    internal ItemLeaf RequestExistingItem(string namedId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        return EnsureNamedIdExists(namedId, Items);
    }

    private static void EnsureNamedIdIsValidEnumName(string namedId)
    {
        Guard.IsNotNullOrWhiteSpace(namedId);
        if (namedId.Trim() != namedId)
            ThrowHelper.ThrowArgumentException(nameof(namedId), $"\"{namedId}\" cannot start or end with whitespaces");

        char firstChar = namedId[0];
        if (char.IsDigit(firstChar) || firstChar == '-' || firstChar == '+')
        {
            ThrowHelper.ThrowArgumentException(
                nameof(namedId),
                $"\"{namedId}\" cannot have its first character be a digit, \"-\" or \"+\"");
        }

        if (namedId.Contains(','))
            ThrowHelper.ThrowArgumentException(nameof(namedId), $"\"{namedId}\" cannot contain any commas (\",\")");
    }

    private static void EnsureNamedIdIsFree<T>(
        string namedId,
        Dictionary<string, T> registry,
        [CallerArgumentExpression(nameof(registry))]
        string registryName = "")
    {
        if (registry.ContainsKey(namedId))
        {
            ThrowHelper.ThrowArgumentException(
                nameof(namedId),
                $"\"{namedId}\" already exists in the {registryName} registry");
        }
    }

    private static T EnsureNamedIdExists<T>(
        string namedId,
        Dictionary<string, T> registry,
        [CallerArgumentExpression(nameof(registry))]
        string registryName = "")
    {
        if (!registry.TryGetValue(namedId, out T content))
        {
            return ThrowHelper.ThrowArgumentException<T>(
                nameof(namedId),
                $"\"{namedId}\" does not exist in the {registryName} registry");
        }

        return content;
    }
}