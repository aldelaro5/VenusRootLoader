namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// A service that provides information about the environment of the bootstrap.
/// </summary>
public sealed class BootstrapEnvironment
{
    /// <summary>
    /// The base directory where variable data are located at. Defaults to the game directory.
    /// </summary>
    public required string BasePath { get; init; }
}