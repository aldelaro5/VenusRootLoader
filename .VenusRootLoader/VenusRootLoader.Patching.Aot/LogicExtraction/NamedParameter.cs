using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

/// <summary>
/// Represents the information needed to declare a parameter to a method.
/// </summary>
public sealed class NamedParameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public required TypeSignature TypeSignature { get; init; }
}