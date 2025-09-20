using Windows.Win32;
using Windows.Win32.System.Console;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Logging;

public sealed class ConsoleLogProvider : ILoggerProvider
{
    public enum RenderingMode
    {
        AnsiColors,
        LegacyColors,
        NoColors
    }

    private readonly ConsoleLoggerSettings _consoleLoggerSettings;
    private readonly RenderingMode _renderingMode;

    public unsafe ConsoleLogProvider(
        GameExecutionContext gameExecutionContext,
        IOptions<ConsoleLoggerSettings> loggingSettings)
    {
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
            var outHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
            var errHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);
            CONSOLE_MODE outMode;
            CONSOLE_MODE errMode;
            PInvoke.GetConsoleMode(outHandle, &outMode);
            PInvoke.GetConsoleMode(errHandle, &errMode);
            outMode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            errMode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            _renderingMode = PInvoke.SetConsoleMode(outHandle, outMode) && PInvoke.SetConsoleMode(errHandle, errMode)
                ? RenderingMode.AnsiColors
                : RenderingMode.LegacyColors;
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!_consoleLoggerSettings.Enable!.Value)
            return NullLogger.Instance;

        return new ConsoleLogger(categoryName, _renderingMode);
    }

    public void Dispose() { }
}
