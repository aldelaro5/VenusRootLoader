using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal class ModLoader : IHostedService
{
    private readonly IModsDiscovery _modsDiscovery;
    private readonly IModsEnumerator _modsEnumerator;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public ModLoader(
        IModsDiscovery modsDiscovery,
        IModsEnumerator modsEnumerator,
        IFileSystem fileSystem,
        ILogger<ModLoader> logger,
        ILoggerFactory loggerFactory)
    {
        _modsDiscovery = modsDiscovery;
        _modsEnumerator = modsEnumerator;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, ModInfo> metadata = new();
        try
        {
            metadata = _modsDiscovery.DiscoverAllMods();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while loading mods");
        }

        foreach (ModInfo modLoadingInfo in _modsEnumerator.EnumerateMods(metadata))
        {
            try
            {
                LoadMod(modLoadingInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while loading the mod {modId}",
                    modLoadingInfo.ModManifest.ModId);
                _modsEnumerator.MarkModAsFailed(modLoadingInfo.ModManifest.ModId);
            }
        }

        return Task.CompletedTask;
    }

    private void LoadMod(ModInfo modLoadingInfo)
    {
        Assembly assembly = Assembly.LoadFrom(modLoadingInfo.ModAssemblyPath);
        Type modType = assembly.GetType(modLoadingInfo.ModType.FullName);
        Mod mod = (Mod)Activator.CreateInstance(modType);
        mod.Logger = _loggerFactory.CreateLogger(modLoadingInfo.ModManifest.ModId);
        mod.BaseModPath = _fileSystem.Path.GetDirectoryName(modLoadingInfo.ModAssemblyPath)!;

        _logger.LogDebug("Loading mod {modId}...", modLoadingInfo.ModManifest.ModId);
        mod.Main();
        _logger.LogDebug("Loaded mod {modId} successfully", modLoadingInfo.ModManifest.ModId);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}