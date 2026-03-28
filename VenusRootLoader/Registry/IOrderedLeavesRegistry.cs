using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

/// <summary>
/// An <see cref="ILeavesRegistry{TLeaf}"/> wrapper that adds the concept of configurable ordering which is a way for
/// leaves to order themselves based on an initial list and some factors that determines their priority in that list.
/// This is used to implement leaves who have a concept of display ordering that don't necessarily match their <see cref="Leaf.GameId"/>
/// order. Every custom leaves is ordered based on the following from most the priority to the least priority:
/// <list type="bullet">
/// <item>A base game leaf's <see cref="Leaf.GameId"/> that the <see cref="Bud"/> decides</item>
/// <item>The <see cref="Bud"/>'s position in the load order</item>
/// <item>A priority that the <see cref="Bud"/> decides (this allows buds to order their leaves between themselves)</item>
/// </list>
/// Because it's a wrapper, it doesn't actually change anything regarding the registration process, it only adds the
/// concept of ordering on top of the registry.
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type of the <see cref="ILeavesRegistry{TLeaf}"/> that this wraps.</typeparam>
internal interface IOrderedLeavesRegistry<TLeaf>
    where TLeaf : Leaf
{
    /// <summary>
    /// The registry wrapped by this ordered registry.
    /// </summary>
    ILeavesRegistry<TLeaf> Registry { get; }

    /// <summary>
    /// The initial ordered list of the leaves.
    /// </summary>
    Dictionary<int, int> BaseGameIdsToOrderingIndex { get; }

    /// <summary>
    /// Calls <see cref="ILeavesRegistry{TLeaf}.RegisterNew"/> while also adding the ordering information of the new leaf.
    /// </summary>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <param name="orderAfterBaseGameId">The preceding game id from the initial list that this leaf should be in the
    /// ordering. If this is null, it means to place it before any other leaves from the initial list.</param>
    /// <param name="orderPriority">The relative priority of the leaf in the ordering</param>
    /// <returns><inheritdoc cref="ILeavesRegistry{TLeaf}.RegisterNew"/></returns>
    TLeaf RegisterNewWithOrdering(string namedId, string creatorId, int? orderAfterBaseGameId, int orderPriority);

    /// <summary>
    /// Calls <see cref="ILeavesRegistry{TLeaf}.RegisterExisting"/> while also adding the ordering information of the new leaf.
    /// </summary>
    /// <param name="gameId">The game id of the new leaf for the game to identify it.</param>
    /// <param name="namedId">The named id of the new leaf for buds to identify it.</param>
    /// <param name="creatorId">The creator id that identifies who authored the leaf.</param>
    /// <returns><inheritdoc cref="ILeavesRegistry{TLeaf}.RegisterExisting"/></returns>
    TLeaf RegisterExistingWithOrdering(int gameId, string namedId, string creatorId);

    /// <summary>
    /// Sets the initial ordering of the leaves.
    /// </summary>
    /// <param name="orderedGameIds">The ordered list of leaf's <see cref="Leaf.GameId"/>.</param>
    void SetBaseGameOrdering(int[] orderedGameIds);

    /// <summary>
    /// Gets a collection containing all the ordered leaves of the registry.
    /// </summary>
    /// <returns>A read only collection containing all the ordered leaves of the registry.</returns>
    IReadOnlyCollection<TLeaf> GetOrderedLeaves();
}