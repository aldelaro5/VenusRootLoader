using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace VenusRootLoader;

internal class BootstrapFunctions
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate void LogMsgFn(string message, string category, LogLevel logLevel);
    internal required LogMsgFn LogMsg { get; init; }
}