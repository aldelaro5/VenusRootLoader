namespace VenusRootLoader.LeavesInternals;

internal interface IHasUnderluingValue<T>
{
    T UnderlyingRef { get; }
}