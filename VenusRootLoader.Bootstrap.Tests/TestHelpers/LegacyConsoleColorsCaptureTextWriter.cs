namespace VenusRootLoader.Bootstrap.Tests.TestHelpers;

public class LegacyConsoleColorsCaptureTextWriter : StringWriter
{
    public override void Write(string? value)
    {
        WriteColorMarker();
        base.Write(value);
    }

    private void WriteColorMarker()
    {
        base.Write("~");
        base.Write(Enum.GetName(Console.ForegroundColor));
        base.Write("~");
    }
}