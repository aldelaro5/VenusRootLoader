using NuGet.Versioning;

namespace VenusRootLoader.Models;

internal sealed class BudManifest
{
    public required string AssemblyName { get; init; }
    public required string BudId { get; init; }
    public required string BudName { get; init; }
    public required NuGetVersion BudVersion { get; init; }
    public required string BudAuthor { get; init; }
    public required BudDependency[] BudDependencies { get; init; }
    public required BudIncompatibility[] BudIncompatibilities { get; init; }
}

internal sealed record BudDependency
{
    public required string BudId { get; set; }
    public required bool Optional { get; set; }
}

internal sealed record BudIncompatibility
{
    public required string BudId { get; set; }
}