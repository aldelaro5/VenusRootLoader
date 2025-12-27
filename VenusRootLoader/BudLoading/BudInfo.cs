using AsmResolver.DotNet;
using VenusRootLoader.Public;

namespace VenusRootLoader.BudLoading;

internal sealed record BudInfo
{
    internal required BudManifest BudManifest { get; init; }
    internal required string BudAssemblyPath { get; init; }
    internal required TypeDefinition BudType { get; init; }
}