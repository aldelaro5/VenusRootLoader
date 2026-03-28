namespace VenusRootLoader.Api;

/// <summary>
/// A class containing information about <see cref="VenusRootLoader"/>'s execution that can be useful for <see cref="Bud"/> to access.
/// </summary>
public sealed class BudLoaderContext
{
    /// <summary>
    /// The full path of the buds directory on disk.
    /// </summary>
    public required string BudsPath { get; init; }

    /// <summary>
    /// The full path of the configuration directory on disk which contains all buds and <see cref="VenusRootLoader"/>'s own configuration files.
    /// </summary>
    public required string ConfigPath { get; init; }

    /// <summary>
    /// The full path of the directory on disk where the assemblies where <see cref="VenusRootLoader"/> resides.
    /// </summary>
    public required string LoaderPath { get; init; }
}