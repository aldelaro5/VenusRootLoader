using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenusRootLoader.Api;

namespace VenusRootLoader.JsonConverters;

/// <summary>
/// A <see cref="JsonConverter"/> for a NuGet's <see cref="VersionRange"/> used in <see cref="Bud"/>'s manifests.
/// </summary>
internal sealed class NuGetVersionRangeJsonConverter : JsonConverter<VersionRange>
{
    internal static readonly NuGetVersionRangeJsonConverter Instance = new();

    public override VersionRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        VersionRange.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, VersionRange value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.OriginalString);
}