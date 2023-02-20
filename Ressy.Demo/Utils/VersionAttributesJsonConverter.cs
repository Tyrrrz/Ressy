using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ressy.HighLevel.Versions;

namespace Ressy.Demo.Utils;

// Dictionaries with non-string keys require a custom converter
internal class VersionAttributesJsonConverter : JsonConverter<IReadOnlyDictionary<VersionAttributeName, string>>
{
    public override IReadOnlyDictionary<VersionAttributeName, string> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        throw new NotSupportedException();

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyDictionary<VersionAttributeName, string> obj,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, value) in obj)
            writer.WriteString(key, value);

        writer.WriteEndObject();
    }
}