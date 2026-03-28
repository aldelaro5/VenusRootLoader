using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.BaseGameCollector;

/// <summary>
/// A service that collects base game data such that the state of <see cref="VenusRootLoader"/> reflects the base game
/// before any <see cref="Bud"/> gets loaded.
/// </summary>
internal interface IBaseGameCollector
{
    /// <summary>
    /// Collects data from the base game.
    /// </summary>
    /// <param name="baseGameId">The identifier to use to register <see cref="Leaf"/> as the creator.</param>
    void CollectBaseGameData(string baseGameId);
}