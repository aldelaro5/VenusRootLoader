using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal class EnumBasedRegistry<TLeaf, TEnum> : BaseRegistry<TLeaf>
    where TLeaf : Leaf, new()
    where TEnum : Enum
{
    private readonly int _offsetEnumValueToGameId;
    private readonly EnumPatcher _enumPatcher;

    public EnumBasedRegistry(int offsetEnumValueToGameId, EnumPatcher enumPatcher, ILogger logger)
        : base(logger)
    {
        _offsetEnumValueToGameId = offsetEnumValueToGameId;
        _enumPatcher = enumPatcher;
    }

    public sealed override TLeaf RegisterExisting(int gameId, string namedId, string creatorId) =>
        base.RegisterExisting(gameId + _offsetEnumValueToGameId, namedId, creatorId);

    protected sealed override int CreateNewGameId(string namedId, string creatorId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        int enumValue = _enumPatcher.AddCustomEnumName(typeof(TEnum), namedId);
        return enumValue + _offsetEnumValueToGameId;
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
}