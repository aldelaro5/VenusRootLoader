using AsmResolver.DotNet;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using VenusRootLoader.JsonConverters;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal interface IBudsDiscoverer
{
    IList<BudInfo> DiscoverAllBudsFromDisk();
}

internal sealed class BudsDiscoverer : IBudsDiscoverer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BudsDiscoverer> _logger;
    private readonly BudLoaderContext _budLoaderContext;

    public BudsDiscoverer(
        IFileSystem fileSystem,
        ILogger<BudsDiscoverer> logger,
        BudLoaderContext budLoaderContext)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _budLoaderContext = budLoaderContext;
    }

    public IList<BudInfo> DiscoverAllBudsFromDisk()
    {
        List<BudInfo> result = new();

        foreach (string budDirectory in _fileSystem.Directory.EnumerateDirectories(_budLoaderContext.BudsPath))
        {
            string manifestPath = _fileSystem.Path.Combine(budDirectory, "manifest.json");
            if (!_fileSystem.File.Exists(manifestPath))
                continue;

            try
            {
                string manifestContent = _fileSystem.File.ReadAllText(manifestPath);
                BudManifest? budManifest = JsonSerializer.Deserialize<BudManifest>(
                    manifestContent,
                    new JsonSerializerOptions { Converters = { NuGetVersionJsonConverter.Instance } });
                if (budManifest is null)
                    throw new JsonException("The bud manifest deserialized to null");

                EnsureBudManifestIsValid(budManifest);

                string budAssemblyPath = _fileSystem.Path.Combine(budDirectory, budManifest.AssemblyName);
                if (!_fileSystem.File.Exists(budAssemblyPath))
                    throw new FileNotFoundException("The bud assembly file does not exist", budAssemblyPath);

                if (TryValidateBudAssembly(budAssemblyPath, out TypeDefinition? budType))
                {
                    BudInfo newBud = new()
                    {
                        BudManifest = budManifest,
                        BudAssemblyPath = budAssemblyPath,
                        BudType = budType
                    };
                    result.Add(newBud);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while reading the bud manifest located at {manifestPath}",
                    manifestPath);
            }
        }

        _logger.LogDebug(
            "Discovered {budCount} mods:\n\n{buds}",
            result.Count,
            string.Join(", ", result.Select(m => m.BudManifest.BudId)));
        return result;
    }

    private static void EnsureBudManifestIsValid(BudManifest budManifest)
    {
        if (string.IsNullOrWhiteSpace(budManifest.AssemblyName))
            throw new ArgumentException($"Invalid {nameof(budManifest.AssemblyName)}: {budManifest.AssemblyName}");
        if (string.IsNullOrWhiteSpace(budManifest.BudId))
            throw new ArgumentException($"Invalid {nameof(budManifest.BudId)}: {budManifest.BudId}");
        if (string.IsNullOrWhiteSpace(budManifest.BudName))
            throw new ArgumentException($"Invalid {nameof(budManifest.BudName)}: {budManifest.BudName}");
        if (string.IsNullOrWhiteSpace(budManifest.BudAuthor))
            throw new ArgumentException($"Invalid {nameof(budManifest.BudAuthor)}: {budManifest.BudAuthor}");
        if (budManifest.BudVersion is null)
            throw new ArgumentException($"{nameof(budManifest.BudVersion)} is null");
    }

    private bool TryValidateBudAssembly(string budAssemblyPath, [NotNullWhen(true)] out TypeDefinition? budType)
    {
        budType = null;

        try
        {
            ModuleDefinition moduleDefinition = ModuleDefinition.FromFile(budAssemblyPath);
            List<TypeDefinition> iBuds = moduleDefinition.GetAllTypes()
                .Where(x => !x.IsAbstract
                            && x.BaseType?.FullName == typeof(Bud).FullName!
                            && x.GetConstructor() is not null)
                .ToList();

            switch (iBuds.Count)
            {
                case <= 0:
                    throw new Exception(
                        $"There are no non abstract classes in the assembly that inherits from {nameof(Bud)} with a parameterless constructor");
                case > 1:
                    throw new Exception(
                        $"There are more than 1 non abstract classes in the assembly that inherits from {nameof(Bud)} with a parameterless constructor " +
                        $"which creates ambiguity. Here are the list of types:\n\n{string.Join("\n", iBuds.Select(x => x.FullName))}");
                default:
                    budType = iBuds.Single();
                    return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "The bud assembly {budAssemblyPath} could not be loaded", budAssemblyPath);
            return false;
        }
    }
}