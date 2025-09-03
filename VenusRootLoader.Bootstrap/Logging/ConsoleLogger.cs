using System.Drawing;
using Microsoft.Extensions.Logging;
using Pastel;
using VenusRootLoader.Bootstrap.Services;

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

    private readonly GameExecutionContext _gameExecutionContext;

    public ConsoleLogger(GameExecutionContext gameExecutionContext, string categoryName, Color? categoryColor)
    {
        _categoryName = categoryName;
        _categoryColor = categoryColor;
        _gameExecutionContext = gameExecutionContext;
        if (_categoryColor is not null)
            _legacyCategoryColor = GetClosestConsoleColor(_categoryColor.Value);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var legacyCategoryColor = _legacyCategoryColor ?? ConsoleColor.White;

        string message = formatter(state, exception);
        if (exception is not null)
            message += $" {exception}";

        // Wine does not support VT100 even if GetConsoleMode advertise that it does and even if SetConsoleMode to enable
        // returns no errors, it does not support ANSI color codes
        if (_gameExecutionContext.IsWine)
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
            Console.Out.Write(_categoryName);

            Console.ResetColor();
            Console.Write("] ");

            Console.ForegroundColor = _logLevelInfos[logLevel].LegacyColor;
            Console.Out.WriteLine($"{message}");

            Console.ResetColor();

            return;
        }

        var categoryColor = _categoryColor ?? Color.White;

        Console.WriteLine($"[{time.Pastel(TimeColor)}] " +
                          $"[{_logLevelInfos[logLevel].Moniker.Pastel(_logLevelInfos[logLevel].Color)}] " +
                          $"[{_categoryName.Pastel(categoryColor)}] " +
                          $"{message.Pastel(_logLevelInfos[logLevel].Color)}");
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