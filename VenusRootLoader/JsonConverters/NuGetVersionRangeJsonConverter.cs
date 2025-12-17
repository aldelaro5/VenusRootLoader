using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VenusRootLoader.JsonConverters;

internal sealed class NuGetVersionRangeJsonConverter : JsonConverter<VersionRange>
{
    internal static readonly NuGetVersionRangeJsonConverter Instance = new();

    public override VersionRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        VersionRange.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, VersionRange value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.OriginalString);
}