namespace VenusRootLoader.Api.Leaves;

public interface ILeafId
{
    /// <summary>
    /// The unique named identifier of the leaf among its type which uniquely identifies the leaf for all buds and
    /// <see cref="VenusRootLoader"/>. If this is a base game leaf, this will be assigned a value by <see cref="VenusRootLoader"/>.
    /// If this is a custom leaf, the value is decided by the <see cref="Bud"/> who registers it.
    /// </summary>
    string NamedId { get; }

    /// <summary>
    /// An identifier that specified who created this leaf. For a base game leaf, the value is always <c>BaseGame</c>.
    /// For a custom leaf, the value is the <see cref="Bud"/>'s id who registered it.
    /// </summary>
    string CreatorId { get; }
}