using Microsoft.Build.Framework;
using NuGet.Versioning;
using System.Text.Json;
using VenusRootLoader.Build.Tasks.JsonConverters;
using Task = Microsoft.Build.Utilities.Task;

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
            throw new ArgumentException($"{AssemblyPath} has no parent directory", AssemblyPath);
        if (!NuGetVersion.TryParse(BudVersion, out NuGetVersion? version))
            throw new ArgumentException($"{BudVersion} is not a valid version", nameof(BudVersion));
        if (BudDependencies.Any(d => d.ItemSpec == BudId))
            throw new ArgumentException("The mod cannot have a dependency with itself", nameof(BudDependencies));
        if (BudIncompatibilities.Any(d => d.ItemSpec == BudId))
            throw new ArgumentException(
                "The mod cannot have an incompatibility with itself",
                nameof(BudIncompatibilities));

        try
        {
            BudDependency[] dependencies = BudDependencies
                .Select(x => new BudDependency
                {
                    BudId = x.ItemSpec,
                    Optional = x.GetMetadata("Optional") == "true"
                })
                .ToArray();
            BudIncompatibility[] incompatibilities = BudIncompatibilities
                .Select(x => new BudIncompatibility { BudId = x.ItemSpec })
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
                    Converters = { NuGetVersionJsonConverter.Instance }
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