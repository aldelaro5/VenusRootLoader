using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Api;

// ReSharper disable UnusedAutoPropertyAccessor.Global
/// <summary>
/// The base class of every bud which is a loadable modding unit of <see cref="VenusRootLoader"/>.
/// </summary>
public abstract class Bud
{
    /// <summary>
    /// An <see cref="ILogger"/> configured with a matching category for the bud.
    /// </summary>
    protected internal ILogger Logger { get; internal set; } = null!;

    /// <summary>
    /// The information that was obtained by parsing the bud's manifest file.
    /// </summary>
    protected internal BudManifest BudInfo { get; internal set; } = null!;

    /// <summary>
    /// The full path on disk where the bud resides in.
    /// </summary>
    protected internal string BaseBudPath { get; internal set; } = null!;

    /// <summary>
    /// The <see cref="Venus"/> instance used to access key <see cref="VenusRootLoader"/> APIs which is tailored for this bud specifically.
    /// </summary>
    protected internal Venus Venus { get; internal set; } = null!;

    /// <summary>
    /// An object containing the configuration data after loading it from the bud's configuration file.
    /// This will be null if <see cref="DefaultConfigData"/> is also null meaning this bud opted out of having configuration data.
    /// </summary>
    protected internal object? ConfigData { get; internal set; }

    /// <summary>
    /// The default configuration data that will be used if no configuration file is found for the bud. If this is null,
    /// it indicates this bud is opting out of having any configuration data which will results in <see cref="ConfigData"/> to always have a value of null.
    /// </summary>
    protected internal virtual object? DefaultConfigData => null;

    /// <summary>
    /// The bud's entrypoint method called by <see cref="VenusRootLoader"/> when the bud is ready to be loaded.
    /// </summary>
    protected internal abstract void Main();
}