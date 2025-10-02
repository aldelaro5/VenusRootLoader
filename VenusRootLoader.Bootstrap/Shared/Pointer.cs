namespace VenusRootLoader.Bootstrap.Shared;

public struct Pointer<T> where T : unmanaged
{
    internal readonly unsafe T* Value;
    public unsafe Pointer(T* value) => Value = value;
}