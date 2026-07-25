namespace VenusRootLoader.LeavesInternals;

/// <summary>
/// A reference type used to wrap a value type so they can be used as a way to refer to a value type by reference instead of by value.
/// </summary>
/// <typeparam name="T">The value type this refers to.</typeparam>
internal sealed class Ref<T>
    where T : struct
{
    /// <summary>
    /// The value referred to by this instance.
    /// </summary>
    internal T Value;

    /// <summary>
    /// Creates a <see cref="Ref{T}"/> from a value.
    /// </summary>
    /// <param name="value">The value the instance will refer to.</param>
    internal Ref(T value) => Value = value;
}