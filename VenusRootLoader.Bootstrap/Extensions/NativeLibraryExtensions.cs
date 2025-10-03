using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Extensions;

// ReSharper disable once UnusedType.Global
internal static class NativeLibraryExtensions
{
    extension(NativeLibrary)
    {
        public static T GetExportDelegate<T>(nint handle, string name) where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(handle, name));
        }
    }
}