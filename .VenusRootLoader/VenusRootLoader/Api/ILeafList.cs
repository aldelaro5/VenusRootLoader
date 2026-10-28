using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api;

/// <summary>
/// A list containing leaves. Unlike a registry, this list is meant to be used directly by buds and offers a lot more control.
/// It is used for leaves that the game makes no assumptions about the <see cref="Leaf.GameId"/> or <see cref="Leaf.NamedId"/>.
/// It has the same properties as a regular registry has meaning a leaf list still serves as the single source of truth for
/// <see cref="VenusRootLoader"/> and the game.
/// </summary>
/// <typeparam name="TLeaf">The type of leaf this list stores.</typeparam>
public interface ILeafList<TLeaf> : IList<TLeaf> where TLeaf : Leaf;