using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client.Serialization
{
    internal sealed class UnitJsonConverter : JsonConverter<Unit>
    {
        public override bool HandleNull => true;

        public override Unit? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Skip();
                return default;
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Read();
                return default;
            }
            else throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
        {
            // TODO(dmitry.azaraev): (Low) UnitJsonConverter: Chrome allow nulls or empty object for empty params (unit type). In CBOR null might be preferable?
            // Add serialization/deserialization tests, for cases which can't be reproduced by chrome.
            // There is still good idea to try to not rely on JsonConverter for this case, and handle this internally,
            // to cover common cases.

            // writer.WriteNullValue();
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}
