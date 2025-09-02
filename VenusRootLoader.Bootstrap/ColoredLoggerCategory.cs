using System.Drawing;

namespace VenusRootLoader.Bootstrap;

public static class ColoredLoggerCategory
{
    private const string Separator = "~";

    public static string Encode(string category, Color color) =>
        $"{color.ToArgb():X}{Separator}{category}";

    public static (string category, Color? color) Decode(string categoryWithColorInfo)
    {
        string[] parts = categoryWithColorInfo.Split(Separator);
        if (parts.Length <= 1)
            return (parts[0], null);
        int color = Convert.ToInt32(parts[0], 16);
        return (parts[1], Color.FromArgb(color));
    }
}