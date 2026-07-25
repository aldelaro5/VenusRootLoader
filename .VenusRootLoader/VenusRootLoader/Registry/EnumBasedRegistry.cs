using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

/// <summary>
/// A registry whose <see cref="Leaf"/>'s <see cref="Leaf.GameId"/> matches a specific <see cref="Enum"/> value and whose
/// <see cref="Leaf.NamedId"/> matches the name of this value.
/// </summary>
/// <typeparam name="TLeaf"><inheritdoc/></typeparam>
/// <typeparam name="TEnum">The <see cref="Enum"/> type that can identify every leaf in this registry.</typeparam>
internal sealed class EnumBasedRegistry<TLeaf, TEnum> : BaseRegistry<TLeaf>
    where TLeaf : Leaf
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

    public override TSubLeaf RegisterExisting<TSubLeaf>(int gameId, string namedId, string creatorId) =>
        base.RegisterExisting<TSubLeaf>(gameId + _offsetEnumValueToGameId, namedId, creatorId);

    protected override int CreateNewGameId(string effectiveId)
    {
        int enumValue = _enumPatcher.AddCustomEnumName(typeof(TEnum), effectiveId);
        return enumValue + _offsetEnumValueToGameId;
    }
}