using Windows.Win32;

namespace VenusRootLoader.Bootstrap;

internal static class WineUtils
{
    public static bool IsWine { get; }

    static WineUtils()
    {
        var hModNtDll = PInvoke.GetModuleHandle("ntdll.dll");
        var wineGetVersion = PInvoke.GetProcAddress(hModNtDll, "wine_get_version");
        IsWine = wineGetVersion != nint.Zero;
    }
}