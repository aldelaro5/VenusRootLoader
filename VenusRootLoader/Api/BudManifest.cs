using NuGet.Versioning;

namespace VenusRootLoader.Api;

public sealed class BudManifest
{
    public required string AssemblyFile { get; init; }
    public required string BudId { get; init; }
    public required string BudName { get; init; }
    public required NuGetVersion BudVersion { get; init; }
    public required string BudAuthor { get; init; }
    public required BudDependency[] BudDependencies { get; init; }
    public required BudIncompatibility[] BudIncompatibilities { get; init; }
}

public sealed record BudDependency
{
    public required string BudId { get; set; }
    public required bool Optional { get; set; }
    public required VersionRange Version { get; set; }
}

public sealed record BudIncompatibility
{
    public required string BudId { get; set; }
    public required VersionRange? Version { get; set; }
}