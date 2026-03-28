using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenusRootLoader.Api;

namespace VenusRootLoader.JsonConverters;

/// <summary>
/// A <see cref="JsonConverter"/> for a NuGet's <see cref="NuGetVersion"/> used in <see cref="Bud"/>'s manifests.
/// </summary>
public sealed class NuGetVersionJsonConverter : JsonConverter<NuGetVersion>
{
    public static NuGetVersionJsonConverter Instance { get; } = new();

    public override NuGetVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        NuGetVersion.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, NuGetVersion value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToFullString());
}