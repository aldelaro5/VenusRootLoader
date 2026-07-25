namespace VenusRootLoader.Bootstrap.Logging;

/// <summary>
/// An abstraction of a simple console interface so we can easily mock it in unit tests
/// </summary>
public interface IConsole
{
    ConsoleColor ForegroundColor { get; set; }
    void WriteLine(string message);
    void Write(string message);
    void Write(char c);
    void ResetColor();
}