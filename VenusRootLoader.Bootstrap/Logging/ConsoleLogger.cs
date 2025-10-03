using Microsoft.Extensions.Logging;
using Pastel;
using System.Drawing;
using System.Reflection;

namespace VenusRootLoader.Bootstrap.Logging;

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

    private static readonly Color TimeColor = Color.LimeGreen;
    private static readonly ConsoleColor LegacyTimeColor = GetClosestConsoleColor(TimeColor);

    private readonly Dictionary<LogLevel, LogLevelInfo> _logLevelInfos = new()
    {
        { LogLevel.Critical, new()
            {
                LegacyColor = ConsoleColor.Red,
                Color = Color.Red,
                Moniker = "!"
            }
        },
        { LogLevel.Error, new()
            {
                LegacyColor = ConsoleColor.Red,
                Color = Color.Red,
                Moniker = "E"
            }
        },
        { LogLevel.Warning, new()
            {
                LegacyColor = ConsoleColor.Yellow,
                Color = Color.Yellow,
                Moniker = "W"
            }
        },
        { LogLevel.Information, new()
            {
                LegacyColor = ConsoleColor.White,
                Color = Color.White,
                Moniker = "I"
            }
        },
        { LogLevel.Debug, new()
            {
                LegacyColor = ConsoleColor.Gray,
                Color = Color.Gray,
                Moniker = "D"
            }
        },
        { LogLevel.Trace, new()
            {
                LegacyColor = ConsoleColor.DarkGray,
                Color = Color.DimGray,
                Moniker = "T"
            }
        }
    };

    private readonly TimeProvider _timeProvider;
    private readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

    public ConsoleLogger(string categoryName, ConsoleLogProvider.RenderingMode renderingMode, TimeProvider timeProvider)
    {
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        _categoryName = simplifiedCategoryName;
        _categoryColor = categoryName switch
        {
            not null when categoryName.Contains(_assemblyName) => Color.Magenta,
            not null when categoryName == "UNITY" => Color.Cyan,
            _ => Color.White
        };
        _renderingMode = renderingMode;
        _timeProvider = timeProvider;
        if (_categoryColor is not null)
            _legacyCategoryColor = GetClosestConsoleColor(_categoryColor.Value);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var time = _timeProvider.GetLocalNow().ToString("HH:mm:ss.fff");
        var legacyCategoryColor = _legacyCategoryColor ?? ConsoleColor.White;

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        if (_renderingMode == ConsoleLogProvider.RenderingMode.LegacyColors)
        {
            Console.ResetColor();
            Console.Write('[');

            Console.ForegroundColor = LegacyTimeColor;
            Console.Write(time);

            Console.ResetColor();
            Console.Write("] [");

            Console.ForegroundColor = _logLevelInfos[logLevel].LegacyColor;
            Console.Write(_logLevelInfos[logLevel].Moniker);

            Console.ResetColor();
            Console.Write("] [");

            Console.ForegroundColor = legacyCategoryColor;
            Console.Write(_categoryName);

            Console.ResetColor();
            Console.Write("] ");

            Console.ForegroundColor = _logLevelInfos[logLevel].LegacyColor;
            Console.WriteLine($"{message}");

            Console.ResetColor();

            return;
        }

        var categoryColor = _categoryColor ?? Color.White;

        if (_renderingMode == ConsoleLogProvider.RenderingMode.AnsiColors)
        {
            Console.WriteLine($"[{time.Pastel(TimeColor)}] " +
                              $"[{_logLevelInfos[logLevel].Moniker.Pastel(_logLevelInfos[logLevel].Color)}] " +
                              $"[{_categoryName.Pastel(categoryColor)}] " +
                              $"{message.Pastel(_logLevelInfos[logLevel].Color)}");
        }
        else
        {
            Console.WriteLine($"[{time}] " +
                              $"[{_logLevelInfos[logLevel].Moniker}] " +
                              $"[{_categoryName}] " +
                              $"{message}");
        }
    }

    private static ConsoleColor GetClosestConsoleColor(Color color)
    {
        ConsoleColor result = 0;

        double lowestSquareDistance = double.MaxValue;

        foreach (var consoleColor in Enum.GetValues<ConsoleColor>())
        {
            var consoleColorName = Enum.GetName(typeof(ConsoleColor), consoleColor)!;
            // DarkYellow doesn't exist in Color, Goldenrod is the closest
            var colorName = Color.FromName(consoleColorName == "DarkYellow" ? "Goldenrod" : consoleColorName);
            var squareDistance = Math.Pow(colorName.R - (double)color.R, 2.0) +
                                 Math.Pow(colorName.G - (double)color.G, 2.0) +
                                 Math.Pow(colorName.B - (double)color.B, 2.0);

            if (squareDistance == 0.0)
                return consoleColor;
            if (!(squareDistance < lowestSquareDistance))
                continue;

            lowestSquareDistance = squareDistance;
            result = consoleColor;
        }
        return result;
    }
}