namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// A struct that wraps a native pointer which workarounds a lot of restrictions around them
/// for unit testing (such as the inability to use them in lambda expressions and mocking issues)
/// </summary>
/// <typeparam name="T">The type of the pointer to wrap</typeparam>
public struct Pointer<T> where T : unmanaged
{
    internal readonly unsafe T* Value;

    /// <summary>
    /// Creates a Pointer wrapper around value.
    /// </summary>
    /// <param name="value">The pointer to wrap</param>
    public unsafe Pointer(T* value) => Value = value;
}