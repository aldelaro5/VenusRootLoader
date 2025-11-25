namespace VenusRootLoader.Build.Tasks;

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

internal sealed class ModDependency
{
    public required string ModId { get; init; }
    public required bool Optional { get; init; }
}

internal sealed class ModIncompatibility
{
    public required string ModId { get; init; }
}
