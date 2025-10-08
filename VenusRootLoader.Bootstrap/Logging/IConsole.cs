namespace VenusRootLoader.Bootstrap.Logging
{
    public interface IConsole
    {
        ConsoleColor ForegroundColor { get; set; }
        void WriteLine(string message);
        void Write(string message);
        void Write(char c);
        void ResetColor();
    }
}