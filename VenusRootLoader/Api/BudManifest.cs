using NuGet.Versioning;

namespace VenusRootLoader.Api;

/// <summary>
/// A class that contains all the information parsed from a <see cref="Bud"/>'s manifest file. The manifest file declares
/// metadata information about a bud and its presence is required for the bud to load.
/// </summary>
public sealed class BudManifest
{
    /// <summary>
    /// The full path of the assembly file where the <see cref="Bud"/> is contained in.
    /// </summary>
    public required string AssemblyFile { get; init; }

    /// <summary>
    /// The unique identifier of the bud. This must be unique across all buds loaded by <see cref="VenusRootLoader"/>.
    /// </summary>
    public required string BudId { get; init; }

    /// <summary>
    /// The friendly display name of the bud.
    /// </summary>
    public required string BudName { get; init; }

    /// <summary>
    /// The version of the bud to load. It must be a semver compatible version number with NuGet's format extensions.
    /// </summary>
    public required NuGetVersion BudVersion { get; init; }

    /// <summary>
    /// The friendly display name of the author of the bud.
    /// </summary>
    public required string BudAuthor { get; init; }

    /// <summary>
    /// A list of dependencies on this bud which will be enforced by <see cref="VenusRootLoader"/> before loading this bud.
    /// </summary>
    public required BudDependency[] BudDependencies { get; init; }

    /// <summary>
    /// A list of incompatibilities that specifies this bud cannot be loaded if any of them are present.
    /// This will be enforced by <see cref="VenusRootLoader"/> before loading the bud.
    /// </summary>
    public required BudIncompatibility[] BudIncompatibilities { get; init; }
}

/// <summary>
/// A dependency of a <see cref="Bud"/> as specified in their <see cref="BudManifest"/>.
/// </summary>
public sealed record BudDependency
{
    /// <summary>
    /// The dependency bud's unique identifier.
    /// </summary>
    public required string BudId { get; set; }

    /// <summary>
    /// Tells if the dependency is optional. If it is, the bud will still be allowed to load, but it will result in a
    /// warning printed in the logs.
    /// </summary>
    public required bool Optional { get; set; }

    /// <summary>
    /// The version range of the dependency which can accept any NuGet version range.
    /// </summary>
    public required VersionRange Version { get; set; }
}

/// <summary>
/// An incompatibility of a <see cref="Bud"/> as specified in their <see cref="BudManifest"/>.
/// </summary>
public sealed record BudIncompatibility
{
    /// <summary>
    /// The incompatible bud's unique identifier.
    /// </summary>
    public required string BudId { get; set; }

    /// <summary>
    /// The version range of the dependency which can accept any NuGet version range.
    /// If this is null, it specified that this incompatibility applies to any version of the bud.
    /// </summary>
    public required VersionRange? Version { get; set; }
}