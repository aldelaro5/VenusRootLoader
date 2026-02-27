using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal abstract class EnumBasedRegistry<TLeaf, TEnum> : BaseRegistry<TLeaf>
    where TLeaf : Leaf, new()
    where TEnum : Enum
{
    private readonly EnumPatcher _enumPatcher;

    protected EnumBasedRegistry(EnumPatcher enumPatcher, ILogger logger)
        : base(logger)
    {
        _enumPatcher = enumPatcher;
    }

    protected sealed override int CreateNewGameId(string namedId, string creatorId)
    {
        EnsureNamedIdIsValidEnumName(namedId);
        return _enumPatcher.AddCustomEnumName(typeof(TEnum), namedId);
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