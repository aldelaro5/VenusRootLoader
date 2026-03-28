namespace VenusRootLoader.Api.Leaves;

/// <summary>
/// The base class of every leaf.
/// <p>
/// A leaf is a moddable content of the game tracked by <see cref="VenusRootLoader"/>
/// in such a way that all leaves will reflect the final state of the game through patching. All leaves starts by reflecting
/// the base game as is, but every <see cref="Bud"/> can registers their own leaves which will be treated the same was as any
/// base game leaves.
/// </p>
/// <p>
/// Leaves cannot be constructed by buds as they must be registered to <see cref="VenusRootLoader"/>'s database.
/// The registration process will return a newly constructed leaf reserved for the bud to configure it.
/// Leaves contain mutable data that allows to edit any of them in
/// such a way that every change done to every leaf will be reflected to the game. This applies no matter who created the leaf.
/// </p>
/// <p>
/// <see cref="VenusRootLoader"/> can guarantee that no registered leaves will conflict with another because each are identified
/// by a game id which is an identifier for the game to tell every leaf of each type apart and <see cref="VenusRootLoader"/>
/// can always reserve free ids for neewly registered leaf. Buds can use each leaf's named id to address them which are
/// decided by the buds for custom leaves. Each leaves also contains an identifier that specifies who created it.
/// </p>
/// </summary>
public abstract class Leaf : ILeafIdentifier
{
    /// <summary>
    /// The unique identifier of the leaf among its type the game uses to uniquely identify them. The value is always determined by
    /// <see cref="VenusRootLoader"/> when the leaf is registered.
    /// </summary>
    public int GameId { get; }

    /// <summary>
    /// The unique named identifier of the leaf among its type which uniquely identifies the leaf for all buds and
    /// <see cref="VenusRootLoader"/>. If this is a base game leaf, this will be assigned a value by <see cref="VenusRootLoader"/>.
    /// If this is a custom leaf, the value is decided by the <see cref="Bud"/> who registers it.
    /// </summary>
    public string NamedId { get; }

    /// <summary>
    /// An identifier that specified who created this leaf. For a base game leaf, the value is always <c>BaseGame</c>.
    /// For a custom leaf, the value is the <see cref="Bud"/>'s id who registered it.
    /// </summary>
    public string CreatorId { get; }

    private protected Leaf(int gameId, string namedId, string creatorId)
    {
        GameId = gameId;
        NamedId = namedId;
        CreatorId = creatorId;
    }
}