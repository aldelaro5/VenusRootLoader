using AsmResolver.DotNet;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsDiscoverer
{
    IList<ModInfo> DiscoverAllMods();
}

internal sealed class ModsDiscoverer : IModsDiscoverer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModsDiscoverer> _logger;
    private readonly ModLoaderContext _modLoaderContext;

    public ModsDiscoverer(
        IFileSystem fileSystem,
        ILogger<ModsDiscoverer> logger,
        ModLoaderContext modLoaderContext)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _modLoaderContext = modLoaderContext;
    }

    public IList<ModInfo> DiscoverAllMods()
    {
        List<ModInfo> result = new();

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
                {
                    ModInfo newMod = new()
                    {
                        ModManifest = modManifest,
                        ModAssemblyPath = modAssemblyPath,
                        ModType = modType
                    };
                    result.Add(newMod);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while reading the mod manifest located at {manifestPath}",
                    manifestPath);
            }
        }

        return result;
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
}