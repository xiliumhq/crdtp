using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization
{
    // This converter used as workaround for issue #2, to ignore string decoding
    // errors, as they might contain invalid data.
    internal sealed class StringJsonConverter : JsonConverter<string>
    {
        public override bool HandleNull => true;

        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            else if (reader.TokenType == JsonTokenType.String)
            {
                try
                {
                    return reader.GetString();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
            else throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        public override string ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert != typeof(string))
                throw new JsonException("Only string keys supported.");

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("PropertyName token expected.");

            // Expect valid string.
            return reader.GetString()!;
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] string value, JsonSerializerOptions options)
            => writer.WritePropertyName(value);
    }
}
