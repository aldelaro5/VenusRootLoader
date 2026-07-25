using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Logging;

public static class ManagedLogsRelay
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate void LogFromMonoManagedFn(string message, string category, LogLevel logLevel);

    internal static readonly LogFromMonoManagedFn RelayLogFunction = RelayLogFromManaged;

    private static ILoggerFactory _loggerFactory = null!;

    public static void Init(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    private static void RelayLogFromManaged(string message, string category, LogLevel logLevel)
    {
        var logger = _loggerFactory.CreateLogger(category);
        logger.Log(logLevel, message);
    }
}