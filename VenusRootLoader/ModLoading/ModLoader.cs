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

    private record ModDependencyErrorInfo
    {
        public required string ModId { get; init; }
        public required bool Optional { get; init; }
        public required string Reason { get; init; }
    }

    private readonly ModLoaderContext _modLoaderContext;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ModLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<string, ModLoadingInfo> _modManifestsByModId = new();
    private IEnumerable<ModLoadingInfo> _orderedMods = [];
    private readonly List<string> _failedMods = [];

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

            _orderedMods = TopologicalSort(_modManifestsByModId.Values);
            _logger.LogInformation(string.Join(" -> ", _orderedMods.Select(x => x.ModManifest.ModId)));

            LoadAllMods();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while loading mods");
        }

        return Task.CompletedTask;
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

    private IEnumerable<ModLoadingInfo> TopologicalSort(IEnumerable<ModLoadingInfo> nodes)
    {
        List<ModLoadingInfo> result = new();

        HashSet<ModLoadingInfo> arrivedBefore = new();
        HashSet<ModLoadingInfo> visited = new();

        foreach (ModLoadingInfo? input in nodes)
        {
            Stack<ModLoadingInfo> currentPath = new();
            if (Visit(input, currentPath))
                continue;

            HashSet<string> uniqueNodesInCyclicPath = new(StringComparer.InvariantCulture);
            List<string> smallestCyclicPathFound = new();
            while (currentPath.Count > 0)
            {
                ModLoadingInfo nodeInCyclicPath = currentPath.Pop();
                smallestCyclicPathFound.Add(nodeInCyclicPath.ModManifest.ModId);
                if (!uniqueNodesInCyclicPath.Add(nodeInCyclicPath.ModManifest.ModId))
                    break;
            }

            smallestCyclicPathFound.Reverse();
            throw new Exception(
                $"Cyclic Dependency detected, it is not possible to load any mods until this is addressed:\n\n" +
                $"{string.Join(" ->\n", smallestCyclicPathFound)}\n");
        }

        return result.Where(mod => _modManifestsByModId.ContainsKey(mod.ModManifest.ModId));

        bool Visit(ModLoadingInfo node, Stack<ModLoadingInfo> currentPath)
        {
            currentPath.Push(node);

            bool beenOnThisNodeBefore = !arrivedBefore.Add(node);
            if (beenOnThisNodeBefore)
            {
                if (!visited.Contains(node))
                    return false;

                currentPath.Pop();
                return true;
            }

            IEnumerable<ModLoadingInfo?> dependencies = GetModDependenciesThatArePresent(node);
            if (dependencies.Any(dependencyNode => !Visit(dependencyNode!, currentPath)))
                return false;

            visited.Add(node);
            result.Add(node);

            currentPath.Pop();
            return true;
        }
    }

    private IEnumerable<ModLoadingInfo?> GetModDependenciesThatArePresent(ModLoadingInfo mod)
    {
        return mod.ModManifest.ModDependencies
            .Select(modDependency => _modManifestsByModId
                .TryGetValue(modDependency.ModId, out ModLoadingInfo? dependency)
                ? dependency
                : null)
            .Where(x => x is not null);
    }

    private void LoadAllMods()
    {
        HashSet<string> modIdsWithSatisfiedDependencies = new(StringComparer.Ordinal);
        HashSet<string> modIdsWithUnsatisfiedRequiredDependencies = new(StringComparer.Ordinal);
        foreach (ModLoadingInfo modLoadingInfo in _orderedMods)
        {
            List<ModDependencyErrorInfo> unsatisfiedDependencies =
                GetUnsatisfiedDependencies(
                    modLoadingInfo,
                    modIdsWithSatisfiedDependencies,
                    modIdsWithUnsatisfiedRequiredDependencies);

            if (unsatisfiedDependencies.Count > 0)
            {
                if (!HandleUnsatisfiedDependencies(unsatisfiedDependencies, modLoadingInfo))
                {
                    modIdsWithUnsatisfiedRequiredDependencies.Add(modLoadingInfo.ModManifest.ModId);
                    continue;
                }
            }

            modIdsWithSatisfiedDependencies.Add(modLoadingInfo.ModManifest.ModId);

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
                _failedMods.Add(modLoadingInfo.ModManifest.ModId);
            }
        }
    }

    private List<ModDependencyErrorInfo> GetUnsatisfiedDependencies(
        ModLoadingInfo modLoadingInfo,
        HashSet<string> modIdsWithSatisfiedDependencies,
        HashSet<string> modIdsWithUnsatisfiedRequiredDependencies)
    {
        List<ModDependencyErrorInfo> unsatisfiedDependencies = new();

        foreach (ModDependency dependency in modLoadingInfo.ModManifest.ModDependencies)
        {
            bool isMissing = !modIdsWithSatisfiedDependencies.Contains(dependency.ModId);
            if (isMissing)
            {
                unsatisfiedDependencies.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = modIdsWithUnsatisfiedRequiredDependencies.Contains(dependency.ModId)
                            ? "This mod has unsatisfied dependencies"
                            : "This mod is missing"
                    });
            }
            else if (_failedMods.Contains(dependency.ModId))
            {
                unsatisfiedDependencies.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = "This mod threw an exception as it was being loaded"
                    });
            }
        }

        return unsatisfiedDependencies;
    }

    private bool HandleUnsatisfiedDependencies(
        List<ModDependencyErrorInfo> unsatisfiedDependencies,
        ModLoadingInfo modLoadingInfo)
    {
        bool allOptionals = unsatisfiedDependencies.All(d => d.Optional);
        if (allOptionals)
        {
            _logger.LogWarning(
                "{Mod} will be loaded, but it has unsatisfied optional dependencies:\n\n{errors}",
                modLoadingInfo.ModManifest.ModId,
                FormatDependenciesErrors(unsatisfiedDependencies));
            return true;
        }

        _logger.LogError(
            "{Mod} will not be loaded because it has unsatisfied dependencies of which at least 1 was not optional:\n\n{errors}",
            modLoadingInfo.ModManifest.ModId,
            FormatDependenciesErrors(unsatisfiedDependencies));
        return false;
    }

    private static string FormatDependenciesErrors(List<ModDependencyErrorInfo> unsatisfiedDependencies)
    {
        IEnumerable<string> errors = unsatisfiedDependencies
            .Select(d => $"{d.ModId} - " +
                         $"{(d.Optional ? "Optional" : "Required")} - " +
                         $"{d.Reason}");
        return string.Join("\n", errors);
    }

    private void LoadMod(ModLoadingInfo modLoadingInfo)
    {
        Assembly assembly = Assembly.LoadFrom(modLoadingInfo.ModAssemblyPath);
        Type modType = assembly.GetType(modLoadingInfo.ModType.FullName);
        Mod mod = (Mod)Activator.CreateInstance(modType);
        mod.Logger = _loggerFactory.CreateLogger(modLoadingInfo.ModManifest.ModId);
        mod.BaseModPath = _fileSystem.Path.GetDirectoryName(modLoadingInfo.ModAssemblyPath)!;

        _logger.LogDebug("Loading mod {modId}...", modLoadingInfo.ModManifest.ModId);
        mod.Main();
        _logger.LogDebug("Loaded mod {modId} sucessfully", modLoadingInfo.ModManifest.ModId);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}