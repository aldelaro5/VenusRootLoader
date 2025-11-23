namespace VenusRootLoader.ModLoading;

public record ModManifest
{
    public required string AssemblyName { get; set; }
    public required string ModId { get; set; }
    public required string ModName { get; set; }
    public required Version ModVersion { get; set; }
    public required string ModAuthor { get; set; }
    public required string[] ModHardDependency { get; set; }
    public required string[] ModSoftDependency { get; set; }
    public required string[] ModIncompatibleWith { get; set; }
}