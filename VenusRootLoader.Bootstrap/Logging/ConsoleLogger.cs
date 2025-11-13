using Microsoft.Extensions.Logging;
using Pastel;
using System.Drawing;
using System.Reflection;

namespace VenusRootLoader.Bootstrap.Logging;

/// <summary>
/// A console logger whose rendering was configured by a <see cref="ConsoleLogProvider"/>
/// It features colors logic support such that each levels uses a different colors in the message and the categories are
/// rendered with a color determined like the following:
/// - Our bootstrap logs uses magenta
/// - Unity player logs (marked with the special UNITY category) uses cyan
/// - Every other categories renders white, but it could be possible to customise this in the future
/// </summary>
public class ConsoleLogger : ILogger
{
    private struct LogLevelInfo
    {
        internal required ConsoleColor LegacyColor { get; init; }
        internal required Color Color { get; init; }
        internal required string Moniker { get; init; }
    }

    private readonly string _categoryName;
    private readonly ConsoleLogProvider.RenderingMode _renderingMode;
    private readonly Color? _categoryColor;
    private readonly ConsoleColor? _legacyCategoryColor;

    private readonly Dictionary<LogLevel, LogLevelInfo> _logLevelInfos = new()
    {
        {
            LogLevel.Critical, new()
            {
                LegacyColor = ConsoleColor.Red,
                Color = Color.Red,
                Moniker = "!"
            }
        },
        {
            LogLevel.Error, new()
            {
                LegacyColor = ConsoleColor.Red,
                Color = Color.Red,
                Moniker = "E"
            }
        },
        {
            LogLevel.Warning, new()
            {
                LegacyColor = ConsoleColor.Yellow,
                Color = Color.Yellow,
                Moniker = "W"
            }
        },
        {
            LogLevel.Information, new()
            {
                LegacyColor = ConsoleColor.White,
                Color = Color.White,
                Moniker = "I"
            }
        },
        {
            LogLevel.Debug, new()
            {
                LegacyColor = ConsoleColor.Gray,
                Color = Color.Gray,
                Moniker = "D"
            }
        },
        {
            LogLevel.Trace, new()
            {
                LegacyColor = ConsoleColor.DarkGray,
                Color = Color.DimGray,
                Moniker = "T"
            }
        }
    };

    private static readonly Dictionary<ConsoleColor, Color> ClosestColorsFromConsoleColors = new()
    {
        [ConsoleColor.Black] = Color.Black,
        [ConsoleColor.DarkBlue] = Color.DarkBlue,
        [ConsoleColor.DarkGreen] = Color.DarkGreen,
        [ConsoleColor.DarkCyan] = Color.DarkCyan,
        [ConsoleColor.DarkRed] = Color.DarkRed,
        [ConsoleColor.DarkMagenta] = Color.DarkMagenta,
        [ConsoleColor.DarkYellow] = Color.Olive,
        [ConsoleColor.Gray] = Color.Gray,
        [ConsoleColor.DarkGray] = Color.DarkGray,
        [ConsoleColor.Blue] = Color.Blue,
        [ConsoleColor.Green] = Color.Green,
        [ConsoleColor.Cyan] = Color.Cyan,
        [ConsoleColor.Red] = Color.Red,
        [ConsoleColor.Magenta] = Color.Magenta,
        [ConsoleColor.Yellow] = Color.Yellow,
        [ConsoleColor.White] = Color.White
    };

    private readonly TimeProvider _timeProvider;
    private readonly IConsole _console;
    private readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

    public ConsoleLogger(
        string categoryName,
        ConsoleLogProvider.RenderingMode renderingMode,
        TimeProvider timeProvider,
        IConsole console)
    {
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        _categoryName = simplifiedCategoryName;
        _categoryColor = categoryName switch
        {
            not null when categoryName.Contains(_assemblyName) => Color.Magenta,
            not null when categoryName.Contains("VenusRootLoader") => Color.CornflowerBlue,
            not null when categoryName == "UNITY" => Color.LimeGreen,
            _ => Color.Cyan
        };
        _renderingMode = renderingMode;
        _timeProvider = timeProvider;
        _console = console;
        if (_categoryColor is not null)
            _legacyCategoryColor = GetClosestConsoleColor(_categoryColor.Value);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var time = _timeProvider.GetLocalNow().ToString("HH:mm:ss.fff");
        var legacyCategoryColor = _legacyCategoryColor ?? ConsoleColor.Cyan;

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        if (_renderingMode == ConsoleLogProvider.RenderingMode.LegacyColors)
        {
            _console.ResetColor();
            _console.Write($"[{time}] [");

            _console.ForegroundColor = _logLevelInfos[logLevel].LegacyColor;
            _console.Write(_logLevelInfos[logLevel].Moniker);

            _console.ResetColor();
            _console.Write("] [");

            _console.ForegroundColor = legacyCategoryColor;
            _console.Write(_categoryName);

            _console.ResetColor();
            _console.Write("] ");

            _console.ForegroundColor = _logLevelInfos[logLevel].LegacyColor;
            _console.WriteLine($"{message}");

            _console.ResetColor();
            return;
        }

        var categoryColor = _categoryColor ?? Color.Cyan;

        if (_renderingMode == ConsoleLogProvider.RenderingMode.AnsiColors)
        {
            _console.WriteLine(
                $"[{time}] " +
                $"[{_logLevelInfos[logLevel].Moniker.Pastel(_logLevelInfos[logLevel].Color)}] " +
                $"[{_categoryName.Pastel(categoryColor)}] " +
                $"{message.Pastel(_logLevelInfos[logLevel].Color)}");
        }
        else
        {
            _console.WriteLine(
                $"[{time}] " +
                $"[{_logLevelInfos[logLevel].Moniker}] " +
                $"[{_categoryName}] " +
                $"{message}");
        }
    }

    private static ConsoleColor GetClosestConsoleColor(Color color)
    {
        return Enum.GetValues<ConsoleColor>()
            .Select(cc => (ConsoleColor: cc, Color: ClosestColorsFromConsoleColors[cc]))
            .MinBy(x => GetHueDistance(color, x.Color) * 0.8 +
                        GetBrightnessDistance(color, x.Color) * 0.2)
            .ConsoleColor;
    }

    private static float GetHueDistance(Color color1, Color color2)
    {
        return Math.Abs(color2.GetHue() - color1.GetHue()) > 180.0f
            ? 360f - Math.Abs(color2.GetHue() - color1.GetHue())
            : Math.Abs(color2.GetHue() - color1.GetHue());
    }

    private static float GetBrightnessDistance(Color color1, Color color2)
    {
        return Math.Abs(
            ((color1.R * 0.299f + color1.G * 0.587f + color1.B * 0.114f) / 256f) -
            ((color2.R * 0.299f + color2.G * 0.587f + color2.B * 0.114f) / 256f));
    }
}