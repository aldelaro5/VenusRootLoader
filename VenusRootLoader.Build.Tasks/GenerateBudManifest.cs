using Microsoft.Build.Framework;
using NuGet.Versioning;
using System.Runtime.CompilerServices;
using System.Text.Json;
using VenusRootLoader.Build.Tasks.JsonConverters;
using Task = Microsoft.Build.Utilities.Task;

[assembly: InternalsVisibleTo("VenusRootLoader.Build.Tasks.Tests")]

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace VenusRootLoader.Build.Tasks;

public sealed class GenerateBudManifest : Task
{
    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string BudId { get; set; }

    [Required]
    public required string BudName { get; set; }

    [Required]
    public required string BudVersion { get; set; }

    [Required]
    public required string BudAuthor { get; set; }

    [Required]
    public required ITaskItem[] BudDependencies { get; set; }

    [Required]
    public required ITaskItem[] BudIncompatibilities { get; set; }

    [Output]
    public string? BudManifestPath { get; set; }

    public override bool Execute()
    {
        string outputPath = Path.GetDirectoryName(AssemblyPath)!;
        if (outputPath is null)
            throw new Exception($"{AssemblyPath} has no parent directory");
        if (!NuGetVersion.TryParse(BudVersion, out NuGetVersion? version))
            throw new Exception($"{BudVersion} is not a valid version");
        if (BudDependencies.Any(d => d.ItemSpec == BudId))
            throw new Exception("The bud cannot have a dependency with itself");
        if (BudIncompatibilities.Any(d => d.ItemSpec == BudId))
            throw new Exception("The bud cannot have an incompatibility with itself");

        foreach (ITaskItem item in BudDependencies)
        {
            bool dependencyHasVersion = item.MetadataNames
                .Cast<string>()
                .Contains(nameof(BudDependency.Version));
            if (!dependencyHasVersion)
                throw new Exception($"The dependency {item.ItemSpec} does not have a version which is required");

            string dependencyVersion = item.GetMetadata(nameof(BudDependency.Version));
            if (!VersionRange.TryParse(dependencyVersion, out VersionRange? _))
            {
                throw new Exception(
                    $"The dependency {item.ItemSpec} has an invalid version specified: {dependencyVersion}");
            }
        }

        foreach (ITaskItem item in BudIncompatibilities)
        {
            bool incompatibilityHasVersion = item.MetadataNames
                .Cast<string>()
                .Contains(nameof(BudIncompatibility.Version));
            if (!incompatibilityHasVersion)
                continue;

            string incompatibilityVersion = item.GetMetadata(nameof(BudIncompatibility.Version));
            if (!VersionRange.TryParse(incompatibilityVersion, out VersionRange? _))
            {
                throw new Exception(
                    $"The incompatibility {item.ItemSpec} has an invalid version specified: {incompatibilityVersion}");
            }
        }

        try
        {
            BudDependency[] dependencies = BudDependencies
                .Select(x => new BudDependency
                {
                    BudId = x.ItemSpec,
                    Optional = x.GetMetadata(nameof(BudDependency.Optional)) == "true",
                    Version = VersionRange.Parse(x.GetMetadata(nameof(BudDependency.Version)))
                })
                .ToArray();
            BudIncompatibility[] incompatibilities = BudIncompatibilities
                .Select(x => new BudIncompatibility
                {
                    BudId = x.ItemSpec,
                    Version = x.MetadataNames.Cast<string>().Contains(nameof(BudIncompatibility.Version))
                        ? VersionRange.Parse(x.GetMetadata(nameof(BudIncompatibility.Version)))
                        : null
                })
                .ToArray();

            BudManifest manifest = new()
            {
                AssemblyName = Path.GetFileName(AssemblyPath),
                BudId = BudId,
                BudName = BudName,
                BudVersion = version,
                BudAuthor = BudAuthor,
                BudDependencies = dependencies,
                BudIncompatibilities = incompatibilities
            };
            string json = JsonSerializer.Serialize(
                manifest,
                new JsonSerializerOptions
                {
                    AllowDuplicateProperties = false,
                    WriteIndented = true,
                    Converters =
                    {
                        NuGetVersionJsonConverter.Instance,
                        NuGetVersionRangeJsonConverter.Instance
                    }
                });
            string tentativeOutputPath = Path.Combine(outputPath, "manifest.json");

            File.WriteAllText(tentativeOutputPath, json);
            BudManifestPath = tentativeOutputPath;
            Log.LogMessage(MessageImportance.High, $"Bud Manifest file generated successfully at {BudManifestPath}");
            Log.LogMessage(MessageImportance.High, $"Here is the generated bud manifest:\n{json}");
            return true;
        }
        catch (Exception e)
        {
            Log.LogError($"An error occurred while writing the bud manifest file:\n{e}");
            return false;
        }
    }
}