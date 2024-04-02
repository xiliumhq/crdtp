using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization;

internal sealed class FloatJsonConverter : JsonConverter<float>
{
    public override bool HandleNull => true;

    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return float.NaN;
        else return (float)reader.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
