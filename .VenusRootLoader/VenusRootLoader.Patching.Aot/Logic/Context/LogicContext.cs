using AsmResolver.DotNet;
using VenusRootLoader.Patching.Aot.ContextSource;

namespace VenusRootLoader.Patching.Aot.Logic.Context;

/// <summary>
/// A construct used by a <see cref="LogicContainer"/> to pass information to inner containers allowing them to change them
/// so they can be commited back to the outer container.
/// </summary>
public sealed class LogicContext
{
    /// <summary>
    /// The information about the fields to pass inside the context.
    /// </summary>
    public List<LogicContextField> ContextFields { get; } = [];

    /// <summary>
    /// The name of the type the context will have.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// After patching the initialize and commit part of the context, this represents the mapping from their <see cref="IContextFieldSource"/>.
    /// to their field in the context instance. This is empty if the context hasn't been patched yet.
    /// </summary>
    public Dictionary<object, FieldDefinition> ContextFieldsMapping { get; } = [];
}