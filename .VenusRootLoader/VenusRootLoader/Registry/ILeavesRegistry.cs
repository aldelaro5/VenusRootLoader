using System.Diagnostics.CodeAnalysis;
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
internal interface ILeavesRegistry<TLeaf> : IEnumerable<TLeaf>
    where TLeaf : Leaf
{
    /// <summary>Gets the number of leaves contained in the registry.</summary>
    /// <returns>The number of leaves contained in the registry.</returns>
    int Count { get; }

    /// <summary>Gets the number of leaves from the base game contained in the registry.</summary>
    /// <returns>The number of leaves from the base game contained in the registry.</returns>
    int CountBaseGame { get; }

    /// <summary>
    /// Creates a newly registered leaf to the registry with an automatically determined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <returns>The newly registered leaf.</returns>
    TLeaf RegisterNew(string creatorId, string namedId);

    /// <summary>
    /// Creates a newly registered leaf using a subtype of <typeparamref name="TLeaf" /> to the registry with an automatically determined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <typeparam name="TSubLeaf">The leaf subtype</typeparam>
    /// <returns>The newly registered leaf.</returns>
    TSubLeaf RegisterNew<TSubLeaf>(string creatorId, string namedId) where TSubLeaf : TLeaf;

    /// <summary>
    /// Creates a newly registered leaf with a predetermined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="gameId">The game id of the new leaf for the game to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <returns>The newly registered leaf.</returns>
    TLeaf RegisterExisting(int gameId, string creatorId, string namedId);

    /// <summary>
    /// Creates a newly registered leaf using a subtype of <typeparamref name="TLeaf" /> with a predetermined <see cref="Leaf.GameId"/>.
    /// </summary>
    /// <param name="gameId">The game id of the new leaf for the game to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <typeparam name="TSubLeaf">The leaf subtype</typeparam>
    /// <returns>The newly registered leaf.</returns>
    TSubLeaf RegisterExisting<TSubLeaf>(int gameId, string creatorId, string namedId) where TSubLeaf : TLeaf;

    /// <summary>
    /// Obtains a leaf from the registry using the parts of an effective id.
    /// </summary>
    /// <param name="creatorId">The creator id of the leaf.</param>
    /// <param name="namedId">The named id of the leaf.</param>
    /// <returns>The leaf if found.</returns>
    /// <exception cref="ArgumentException">Thrown if the leaf doesn't exist.</exception>
    TLeaf Get(string creatorId, string namedId);

    /// <summary>
    /// Obtains a leaf from the registry.
    /// </summary>
    /// <param name="creatorId">The creator id of the leaf.</param>
    /// <param name="namedId">The named id of the leaf.</param>
    /// <param name="leaf">When this method returns, the leaf associated with the specified <paramref name="namedId"/> and <paramref name="creatorId"/>,
    /// if the leaf is found; otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>True if the leaf is found, false otherwise.</returns>
    bool TryGet(string creatorId, string namedId, [NotNullWhen(true)] out TLeaf? leaf);

    /// <summary>
    /// Obtains a leaf from the registry using an effective id.
    /// </summary>
    /// <param name="effectiveId">The effective id of the leaf.</param>
    /// <returns>The leaf if found.</returns>
    /// <exception cref="ArgumentException">Thrown if the leaf doesn't exist.</exception>
    TLeaf GetByEffectiveId(string effectiveId);

    /// <summary>
    /// Obtains a leaf from the registry using an effective id.
    /// </summary>
    /// <param name="effectiveId">The effective id of the leaf.</param>
    /// <param name="leaf">When this method returns, the leaf associated with the specified <paramref name="effectiveId"/>,
    /// if the leaf is found; otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>True if the leaf is found, false otherwise.</returns>
    bool TryGetByEffectiveId(string effectiveId, [NotNullWhen(true)] out TLeaf? leaf);

    /// <summary>
    /// Obtains a leaf from the registry using its game id.
    /// </summary>
    /// <param name="gameId">The game id of the leaf.</param>
    /// <returns>The leaf if found.</returns>
    /// <exception cref="ArgumentException">Thrown if the leaf doesn't exist.</exception>
    TLeaf GetByGameId(int gameId);

    /// <summary>
    /// Obtains a read only copy of a collection containing all the leaves in the registry.
    /// </summary>
    /// <returns>A collection containing all the leaves of the registry.</returns>
    IReadOnlyCollection<TLeaf> GetAll();
}