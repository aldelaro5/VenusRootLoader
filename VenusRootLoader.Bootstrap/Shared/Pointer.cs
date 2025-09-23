namespace VenusRootLoader.Bootstrap.Shared;

public struct Pointer<T> where T : unmanaged
{
    private readonly unsafe T* _value;
    public unsafe Pointer(T* value) => _value = value;
    public static unsafe implicit operator T*(Pointer<T> pointer) => pointer._value;
}