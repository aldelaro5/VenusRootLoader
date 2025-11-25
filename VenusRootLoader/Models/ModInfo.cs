using AsmResolver.DotNet;

namespace VenusRootLoader.Models;

internal sealed record ModInfo
{
    internal required ModManifest ModManifest { get; init; }
    internal required string ModAssemblyPath { get; init; }
    internal required TypeDefinition ModType { get; init; }
}