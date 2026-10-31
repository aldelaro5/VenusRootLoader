namespace VenusRootLoader.Patching.Aot.LogicExtraction;

/// <summary>
/// A parameter to an inner container that doesn't require a context, but can be sourced from a context source.
/// </summary>
public sealed class LogicParameter
{
    /// <summary>
    /// The sources to pull the value of this parameter.
    /// </summary>
    public required List<ILogicContextFieldSource> ContextFieldSources { get; init; }

    /// <summary>
    /// The parameter info that will appear in the inner container.
    /// </summary>
    public required NamedParameter NamedParameter { get; init; }
}