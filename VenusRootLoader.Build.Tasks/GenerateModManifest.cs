using Microsoft.Build.Framework;
using System.Text.Json;
using Task = Microsoft.Build.Utilities.Task;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace VenusRootLoader.Build.Tasks;

public class GenerateModManifest : Task
{
    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string ModId { get; set; }

    [Required]
    public required string ModName { get; set; }

    [Required]
    public required string ModVersion { get; set; }

    [Required]
    public required string ModAuthor { get; set; }

    [Required]
    public required ITaskItem[] ModDependencies { get; set; }

    [Required]
    public required ITaskItem[] ModIncompatibilities { get; set; }

    [Output]
    public string? ModManifestPath { get; set; }

    public override bool Execute()
    {
        string outputPath = Path.GetDirectoryName(AssemblyPath)!;
        if (outputPath is null)
            throw new ArgumentException($"{AssemblyPath} has no parent directory", AssemblyPath);
        if (!Version.TryParse(ModVersion, out Version? version))
            throw new ArgumentException($"{ModVersion} is not a valid version", nameof(ModVersion));

        try
        {
            ModDependency[] dependencies = ModDependencies
                .Select(x => new ModDependency
                {
                    ModId = x.ItemSpec,
                    Optional = x.GetMetadata("Optional") == "true"
                })
                .ToArray();
            ModIncompatibility[] incompatibilities = ModIncompatibilities
                .Select(x => new ModIncompatibility { ModId = x.ItemSpec })
                .ToArray();
            
            ModManifest manifest = new()
            {
                AssemblyName = Path.GetFileName(AssemblyPath),
                ModId = ModId,
                ModName = ModName,
                ModVersion = version,
                ModAuthor = ModAuthor,
                ModDependencies = dependencies,
                ModIncompatibilities = incompatibilities
            };
            string json = JsonSerializer.Serialize(
                manifest,
                new JsonSerializerOptions
                {
                    AllowDuplicateProperties = false,
                    WriteIndented = true
                });
            string tentativeOutputPath = Path.Combine(outputPath, "manifest.json");

            File.WriteAllText(tentativeOutputPath, json);
            ModManifestPath = tentativeOutputPath;
            Log.LogMessage(MessageImportance.High, $"Mod Manifest file generated successfully at {ModManifestPath}");
            Log.LogMessage(MessageImportance.High, $"Here is the generated mod manifest:\n{json}");
            return true;
        }
        catch (Exception e)
        {
            Log.LogError($"An error occurred while writing the mod manifest file:\n{e}");
            return false;
        }
    }
}