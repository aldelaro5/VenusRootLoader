using VenusRootLoader.Patching.Aot.ContextSource;

namespace VenusRootLoader.Patching.Aot.Logic.Context;

/// <summary>
/// Represents a context field of a <see cref="LogicContext"/>.
/// </summary>
public sealed class LogicContextField
{
    /// <summary>
    /// The sources where the field value will come from and be commited into.
    /// </summary>
    public required List<IContextFieldSource> ContextFieldSources { get; init; }

    /// <summary>
    /// The name the context field will be mapped to. This can be different from a name of the
    /// <see cref="ContextFieldSources"/> if it has one.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// Whether the context field is only read from the inner container and never commited back to the
    /// outer container.
    /// </summary>
    public required bool ReadOnly { get; init; }
}