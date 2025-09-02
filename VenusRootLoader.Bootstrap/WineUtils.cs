namespace VenusRootLoader.Bootstrap;

internal static class WineUtils
{
    public static bool IsWine { get; }

    static WineUtils()
    {
        var hModNtDll = WindowsNative.GetModuleHandleW("ntdll.dll");
        var wineGetVersion = WindowsNative.GetProcAddress(hModNtDll, "wine_get_version");
        IsWine = wineGetVersion != nint.Zero;
    }
}