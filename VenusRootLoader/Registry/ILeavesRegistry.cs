using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Registry;

// TODO: Strongly considers adding a TryGet method

/// <summary>
/// A <see cref="Leaf"/> registry is an in memory database of every leaves that were either
/// registred by an <see cref="IBaseGameCollector"/> or by a <see cref="Bud"/>.
/// The goal of the registry is to become the single source of truth for all buds and the game itself.
/// This has the interesting property that any operations done on a registry or its leaves will be the same
/// no matter where a leaf comes from. More concretely, it means that editing or registering a base game leaf
/// is the same operation as editing or registering a custom leaf (even if that custom leaf wasn't authored by the
/// bud who edits it).
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type this registry manages.</typeparam>
internal interface ILeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    /// <summary>
    /// All leaves of the registry indexed by their <see cref="Leaf.NamedId"/>.
    /// </summary>
    IDictionary<string, TLeaf> LeavesByNamedIds { get; }

    /// <summary>
    /// All leaves of the registry indexed by their <see cref="Leaf.GameId"/>.
    /// </summary>
    IDictionary<int, TLeaf> LeavesByGameIds { get; }

    /// <summary>
    /// Creates a newly registered leaf to the registry with an automatically determined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <returns>The newly registered leaf.</returns>
    TLeaf RegisterNew(string namedId, string creatorId);

    /// <summary>
    /// Creates a newly registered leaf with a predermined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="gameId">The game id of the new leaf for the game to identify it.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <returns>The newly registered leaf.</returns>
    TLeaf RegisterExisting(int gameId, string namedId, string creatorId);

    /// <summary>
    /// Obtains a leaf from the registry.
    /// </summary>
    /// <param name="namedId">The named id of the leaf.</param>
    /// <returns>The leaf if found.</returns>
    /// <exception cref="ArgumentException">Thrown if the leaf doesn't exist.</exception>
    TLeaf Get(string namedId);

    /// <summary>
    /// Obtains a read only copy of a collection containing all the leaves in the registry.
    /// </summary>
    /// <returns>A collection containing all the leaves of the registry.</returns>
    IReadOnlyCollection<TLeaf> GetAll();
}