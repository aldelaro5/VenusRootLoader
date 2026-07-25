using AsmResolver.DotNet;
using VenusRootLoader.Api;

namespace VenusRootLoader.BudLoading;

/// <summary>
/// Contains information about a <see cref="Bud"/> for loading purposes.
/// </summary>
internal sealed record BudInfo
{
    /// <summary>
    /// The information parsed from the <see cref="Bud"/>'s manifest.
    /// </summary>
    internal required BudManifest BudManifest { get; init; }

    /// <summary>
    /// The full path of the assembly file containing the <see cref="Bud"/>.
    /// </summary>
    internal required string BudAssemblyPath { get; init; }

    /// <summary>
    /// The type definition of the <see cref="Bud"/>.
    /// </summary>
    internal required TypeDefinition BudType { get; init; }
}