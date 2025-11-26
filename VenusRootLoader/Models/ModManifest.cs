namespace VenusRootLoader.Models;

internal sealed class ModManifest
{
    public required string AssemblyName { get; init; }
    public required string ModId { get; init; }
    public required string ModName { get; init; }
    public required Version ModVersion { get; init; }
    public required string ModAuthor { get; init; }
    public required ModDependency[] ModDependencies { get; init; }
    public required ModIncompatibility[] ModIncompatibilities { get; init; }
}

internal sealed record ModDependency
{
    public required string ModId { get; set; }
    public required bool Optional { get; set; }
}

internal sealed record ModIncompatibility
{
    public required string ModId { get; set; }
}