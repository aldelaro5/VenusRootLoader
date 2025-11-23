using AsmResolver.DotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;

namespace VenusRootLoader.ModLoading;

public class ModLoader : IHostedService
{
    private record ModLoadingInfo(ModManifest ModManifest, string ModAssemblyPath, TypeDefinition ModType);

    private readonly ModLoaderContext _modLoaderContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<string, ModLoadingInfo> _modManifestsByModId = new();
    private IEnumerable<ModLoadingInfo> _orderedMods = [];
    private List<ModLoadingInfo> _failedMods = [];

    public ModLoader(
        ModLoaderContext modLoaderContext,
        IFileSystem fileSystem,
        ILogger<ModLoader> logger,
        ILoggerFactory loggerFactory)
    {
        _modLoaderContext = modLoaderContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            CollectAllModsMetadata();
            DetermineModsLoadOrder();
            LoadAllMods();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while loading mods");
        }

        return Task.CompletedTask;
    }

    private void LoadAllMods()
    {
        foreach (ModLoadingInfo modLoadingInfo in _orderedMods)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(modLoadingInfo.ModAssemblyPath);
                Type modType = assembly.GetType(modLoadingInfo.ModType.FullName);
                Mod mod = (Mod)Activator.CreateInstance(modType);
                mod.Logger = _loggerFactory.CreateLogger(modLoadingInfo.ModManifest.ModId);
                mod.BaseModPath = _fileSystem.Path.GetDirectoryName(modLoadingInfo.ModAssemblyPath)!;
                mod.Main();
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while loading the mod {modId}",
                    modLoadingInfo.ModManifest.ModId);
                _failedMods.Add(modLoadingInfo);
            }
        }
    }

    private void DetermineModsLoadOrder()
    {
        _orderedMods = _modManifestsByModId.Values.ToList();
    }

    private bool TryValidateModAssembly(string modAssemblyPath, [NotNullWhen(true)] out TypeDefinition? modType)
    {
        modType = null;

        try
        {
            ModuleDefinition moduleDefinition = ModuleDefinition.FromFile(modAssemblyPath);
            List<TypeDefinition> iMods = moduleDefinition.GetAllTypes()
                .Where(x => !x.IsAbstract
                            && x.BaseType?.FullName == typeof(Mod).FullName!
                            && x.GetConstructor() is not null)
                .ToList();

            switch (iMods.Count)
            {
                case <= 0:
                    throw new Exception(
                        $"There are no non abstract classes in the assembly that inherits from {nameof(Mod)} with a parameterless constructor");
                case > 1:
                    throw new Exception(
                        $"There are more than 1 non abstract classes in the assembly that inherits from {nameof(Mod)} with a parameterless constructor " +
                        $"which creates ambiguity. Here are the list of types: {string.Join(",", iMods.Select(x => x.FullName))}");
                default:
                    modType = iMods.Single();
                    return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "The mod assembly {modAssemblyPath} could not be loaded", modAssemblyPath);
            return false;
        }
    }

    private void CollectAllModsMetadata()
    {
        foreach (string modDirectory in _fileSystem.Directory.EnumerateDirectories(_modLoaderContext.ModsPath))
        {
            string manifestPath = _fileSystem.Path.Combine(modDirectory, "manifest.json");
            if (!_fileSystem.File.Exists(manifestPath))
                continue;

            try
            {
                string manifestContent = _fileSystem.File.ReadAllText(manifestPath);
                ModManifest? modManifest = JsonSerializer.Deserialize<ModManifest>(
                    manifestContent,
                    new JsonSerializerOptions());
                if (modManifest is null)
                    throw new JsonException("The mod manifest deserialized to null");

                EnsureModManifestIsValid(modManifest);

                string modAssemblyPath = _fileSystem.Path.Combine(modDirectory, modManifest.AssemblyName);
                if (!_fileSystem.File.Exists(modAssemblyPath))
                    throw new FileNotFoundException("The mod assembly file does not exist", modAssemblyPath);

                if (TryValidateModAssembly(modAssemblyPath, out TypeDefinition? modType))
                    _modManifestsByModId[modManifest.ModId] = new(modManifest, modAssemblyPath, modType);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while reading the mod manifest located at {manifestPath}",
                    manifestPath);
            }
        }
    }

    private void EnsureModManifestIsValid(ModManifest modManifest)
    {
        if (string.IsNullOrWhiteSpace(modManifest.AssemblyName))
            throw new ArgumentException($"Invalid {nameof(modManifest.AssemblyName)}: {modManifest.AssemblyName}");
        if (string.IsNullOrWhiteSpace(modManifest.ModId))
            throw new ArgumentException($"Invalid {nameof(modManifest.ModId)}: {modManifest.ModId}");
        if (string.IsNullOrWhiteSpace(modManifest.ModName))
            throw new ArgumentException($"Invalid {nameof(modManifest.ModName)}: {modManifest.ModName}");
        if (string.IsNullOrWhiteSpace(modManifest.ModAuthor))
            throw new ArgumentException($"Invalid {nameof(modManifest.ModAuthor)}: {modManifest.ModAuthor}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}