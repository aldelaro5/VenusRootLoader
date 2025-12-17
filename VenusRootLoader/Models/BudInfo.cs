using AsmResolver.DotNet;

namespace VenusRootLoader.Models;

internal sealed record BudInfo
{
    internal required BudManifest BudManifest { get; init; }
    internal required string BudAssemblyPath { get; init; }
    internal required TypeDefinition BudType { get; init; }
}