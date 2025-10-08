namespace VenusRootLoader.Bootstrap.Logging
{
    public class SystemConsole : IConsole
    {
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Write(string message) => Console.Write(message);
        public void Write(char c) => Console.Write(c);
        public void ResetColor() => Console.ResetColor();
    }
}