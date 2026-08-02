namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// A service that provides information about the environment of the bootstrap.
/// </summary>
public interface IBootstrapEnvironment
{
    /// <summary>
    /// The base directory where variable data are located at. Defaults to the game directory.
    /// </summary>
    string BasePath { get; init; }
}

/// <inheritdoc/>
public sealed class BootstrapEnvironment : IBootstrapEnvironment
{
    /// <summary>
    /// The base directory where variable data are located at. Defaults to the game directory.
    /// </summary>
    public required string BasePath { get; init; }
}