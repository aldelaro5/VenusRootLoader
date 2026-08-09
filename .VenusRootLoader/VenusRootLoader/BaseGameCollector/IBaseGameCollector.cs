using VenusRootLoader.Api;

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
    void CollectBaseGameData();
}