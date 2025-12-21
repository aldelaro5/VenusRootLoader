using CommunityToolkit.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace VenusRootLoader.JsonConverters;

public sealed class Vector2JsonConverter : JsonConverter<Vector2>
{
    private static readonly char[] Separator = [','];
    public static Vector2JsonConverter Instance { get; } = new();

    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector2>(
                $"A {nameof(Vector2)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 2)
        {
            ThrowHelper.ThrowFormatException<Vector2>(
                $"A {nameof(Vector2)} value must have 2 components separated by commas");
        }

        return new Vector2(
            float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[1], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class Vector2IntJsonConverter : JsonConverter<Vector2Int>
{
    private static readonly char[] Separator = [','];
    public static Vector2IntJsonConverter Instance { get; } = new();

    public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector2Int>(
                $"A {nameof(Vector2Int)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 2)
        {
            ThrowHelper.ThrowFormatException<Vector2Int>(
                $"A {nameof(Vector2Int)} value must have 2 components separated by commas");
        }

        return new Vector2Int(
            int.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            int.Parse(valuesSplit[1], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString(CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    private static readonly char[] Separator = [','];
    public static Vector3JsonConverter Instance { get; } = new();

    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector3>(
                $"A {nameof(Vector3)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 3)
        {
            ThrowHelper.ThrowFormatException<Vector3>(
                $"A {nameof(Vector3)} value must have 3 components separated by commas");
        }

        return new Vector3(
            float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[2], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.z.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class Vector3IntJsonConverter : JsonConverter<Vector3Int>
{
    private static readonly char[] Separator = [','];
    public static Vector3IntJsonConverter Instance { get; } = new();

    public override Vector3Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector3Int>(
                $"A {nameof(Vector3Int)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 3)
        {
            ThrowHelper.ThrowFormatException<Vector3Int>(
                $"A {nameof(Vector3Int)} value must have 3 components separated by commas");
        }

        return new Vector3Int(
            int.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            int.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
            int.Parse(valuesSplit[2], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Vector3Int value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.z.ToString(CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class Vector4JsonConverter : JsonConverter<Vector4>
{
    private static readonly char[] Separator = [','];
    public static Vector4JsonConverter Instance { get; } = new();

    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector4>(
                $"A {nameof(Vector4)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 4)
        {
            ThrowHelper.ThrowFormatException<Vector4>(
                $"A {nameof(Vector4)} value must have 4 components separated by commas");
        }

        return new Vector4(
            float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[2], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[3], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.z.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.w.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class QuaternionJsonConverter : JsonConverter<Quaternion>
{
    private static readonly char[] Separator = [','];
    public static QuaternionJsonConverter Instance { get; } = new();

    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<char> bytes = reader.GetString().AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector4>(
                $"A {nameof(Vector4)} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (valuesSplit.Length != 4)
        {
            ThrowHelper.ThrowFormatException<Vector4>(
                $"A {nameof(Vector4)} value must have 4 components separated by commas");
        }

        return new Quaternion(
            float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[2], CultureInfo.InvariantCulture),
            float.Parse(valuesSplit[3], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        StringBuilder sb = new("(");

        sb.Append(value.x.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.y.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.z.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(value.w.ToString("G9", CultureInfo.InvariantCulture));
        sb.Append(')');

        writer.WriteStringValue(sb.ToString());
    }
}

public sealed class RectJsonConverter : JsonConverter<Rect>
{
    public static RectJsonConverter Instance { get; } = new();

    public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement jsonElement = JsonElement.ParseValue(ref reader);
        return new Rect(
            jsonElement.GetProperty("x").GetSingle(),
            jsonElement.GetProperty("y").GetSingle(),
            jsonElement.GetProperty("width").GetSingle(),
            jsonElement.GetProperty("height").GetSingle());
    }

    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("x");
        writer.WriteNumberValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteNumberValue(value.y);
        writer.WritePropertyName("width");
        writer.WriteNumberValue(value.width);
        writer.WritePropertyName("height");
        writer.WriteNumberValue(value.height);

        writer.WriteEndObject();
    }
}

public sealed class ColorJsonConverter : JsonConverter<Color>
{
    public static ColorJsonConverter Instance { get; } = new();

    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!ColorUtility.TryParseHtmlString(reader.GetString(), out Color value))
        {
            ThrowHelper.ThrowFormatException<Color>(
                $"A {nameof(Color)} value must be in the format #RRGGBBAA with hexadecimal components");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{ColorUtility.ToHtmlStringRGBA(value)}");
    }
}