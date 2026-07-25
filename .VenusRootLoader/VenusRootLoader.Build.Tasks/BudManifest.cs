using NuGet.Versioning;

namespace VenusRootLoader.Build.Tasks;

internal sealed class BudManifest
{
    public required string AssemblyFile { get; init; }
    public required string BudId { get; init; }
    public required string BudName { get; init; }
    public required NuGetVersion BudVersion { get; init; }
    public required string BudAuthor { get; init; }
    public required BudDependency[] BudDependencies { get; init; }
    public required BudIncompatibility[] BudIncompatibilities { get; init; }
}

internal sealed class BudDependency
{
    public required string BudId { get; init; }
    public required bool Optional { get; init; }
    public required VersionRange Version { get; set; }
}

internal sealed class BudIncompatibility
{
    public required string BudId { get; init; }
    public required VersionRange? Version { get; set; }
}
