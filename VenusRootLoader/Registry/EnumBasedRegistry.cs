using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal abstract class EnumBasedRegistry<TLeaf, TEnum> : ILeavesRegistry<TLeaf, int>
    where TLeaf : ILeaf<int>, new()
    where TEnum : Enum
{
    private readonly EnumPatcher _enumPatcher;
    private readonly string _registryName = typeof(TLeaf).Name;

    protected EnumBasedRegistry(EnumPatcher enumPatcher)
    {
        _enumPatcher = enumPatcher;
    }

    public IDictionary<string, TLeaf> Registry { get; } = new Dictionary<string, TLeaf>();

    public TLeaf RegisterNew(string namedId, string creatorId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        EnsureNamedIdIsFree(namedId);
        int newId = _enumPatcher.AddCustomEnumName(typeof(TEnum), namedId);
        TLeaf itemLeaf = new()
        {
            GameId = newId,
            CreatorId = creatorId,
            NamedId = namedId
        };
        Registry[namedId] = itemLeaf;
        return itemLeaf;
    }

    public TLeaf RegisterExisting(int gameId, string namedId, string creatorId)
    {
        TLeaf itemLeaf = new()
        {
            GameId = gameId,
            NamedId = namedId,
            CreatorId = creatorId
        };
        Registry[namedId] = itemLeaf;
        return itemLeaf;
    }

    public TLeaf Get(string namedId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        return EnsureNamedIdExists(namedId);
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

    private void EnsureNamedIdIsFree(string namedId)
    {
        if (Registry.ContainsKey(namedId))
        {
            ThrowHelper.ThrowArgumentException(
                nameof(namedId),
                $"\"{namedId}\" already exists in the {_registryName} registry");
        }
    }

    private TLeaf EnsureNamedIdExists(
        string namedId)
    {
        if (!Registry.TryGetValue(namedId, out TLeaf content))
        {
            return ThrowHelper.ThrowArgumentException<TLeaf>(
                nameof(namedId),
                $"\"{namedId}\" does not exist in the {_registryName} registry");
        }

        return content;
    }
}