using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Logging;

public sealed class ConsoleLogProvider : ILoggerProvider
{
    public enum RenderingMode
    {
        AnsiColors,
        LegacyColors,
        NoColors
    }

    private static readonly SystemConsole SystemConsole = new();

    private readonly TimeProvider _timeProvider;
    private readonly ConsoleLoggerSettings _consoleLoggerSettings;
    private readonly RenderingMode _renderingMode;

    public unsafe ConsoleLogProvider(
        GameExecutionContext gameExecutionContext,
        IOptions<ConsoleLoggerSettings> loggingSettings,
        IWin32 win32,
        TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _consoleLoggerSettings = loggingSettings.Value;

        if (!_consoleLoggerSettings.LogWithColors!.Value)
        {
            _renderingMode = RenderingMode.NoColors;
        }
        // Wine does not support VT100 even if GetConsoleMode advertise that it does and even if SetConsoleMode to enable
        // returns no errors, it does not support ANSI color codes
        else if (gameExecutionContext.IsWine)
        {
            _renderingMode = RenderingMode.LegacyColors;
        }
        else
        {
            var outHandle = win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
            var errHandle = win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);
            CONSOLE_MODE outMode;
            CONSOLE_MODE errMode;
            win32.GetConsoleMode(outHandle, new(&outMode));
            win32.GetConsoleMode(errHandle, new(&errMode));
            outMode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            errMode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            _renderingMode = win32.SetConsoleMode(outHandle, outMode) && win32.SetConsoleMode(errHandle, errMode)
                ? RenderingMode.AnsiColors
                : RenderingMode.LegacyColors;
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!_consoleLoggerSettings.Enable!.Value)
            return NullLogger.Instance;

        return new ConsoleLogger(categoryName, _renderingMode, _timeProvider, SystemConsole);
    }

    public void Dispose() { }
}