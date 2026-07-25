using VenusRootLoader.Bootstrap.Logging;

namespace VenusRootLoader.Bootstrap.Tests.TestHelpers
{
    public sealed class TestConsole : IConsole
    {
        private readonly TextWriter _writer;

        public TestConsole(TextWriter writer) => _writer = writer;

        public bool WriteLegacyColorMarkers { get; set; }
        public ConsoleColor ForegroundColor { get; set; }

        public void WriteLine(string message)
        {
            if (WriteLegacyColorMarkers)
                WriteColorMarker();
            _writer.WriteLine(message);
        }

        public void Write(string message)
        {
            if (WriteLegacyColorMarkers)
                WriteColorMarker();
            _writer.Write(message);
        }

        public void Write(char c)
        {
            if (WriteLegacyColorMarkers)
                WriteColorMarker();
            _writer.Write(c);
        }

        public void ResetColor() => ForegroundColor = (ConsoleColor)(-1);

        private void WriteColorMarker()
        {
            _writer.Write("~");
            _writer.Write(Enum.GetName(ForegroundColor)!);
            _writer.Write("~");
        }
    }
}