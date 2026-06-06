namespace VenusRootLoader.LeavesInternals;

internal sealed class Ref<T>
    where T : struct
{
    internal T Value;
    internal Ref(T value) => Value = value;
}