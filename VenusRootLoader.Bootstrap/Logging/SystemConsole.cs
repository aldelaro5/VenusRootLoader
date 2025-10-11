namespace VenusRootLoader.Bootstrap.Logging;

/// <summary>
/// The real implementation of <see cref="IConsole"/> that uses the <see cref="Console"/> class
/// </summary>
public class SystemConsole : IConsole
{
    public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
    public void WriteLine(string message) => Console.WriteLine(message);
    public void Write(string message) => Console.Write(message);
    public void Write(char c) => Console.Write(c);
    public void ResetColor() => Console.ResetColor();
}