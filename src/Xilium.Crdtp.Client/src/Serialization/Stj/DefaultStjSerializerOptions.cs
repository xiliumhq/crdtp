using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization
{
    internal sealed class DefaultStjSerializerOptions : StjSerializerOptions
    {
        internal static JsonSerializerOptions CreateJsonSerializerOptions()
            => new JsonSerializerOptions
            {
                AllowTrailingCommas = StjOptions.ReaderOptions.AllowTrailingCommas,
                ReadCommentHandling = StjOptions.ReaderOptions.CommentHandling,
                MaxDepth = StjOptions.ReaderOptions.MaxDepth,

                WriteIndented = StjOptions.WriterOptions.Indented,
                Encoder = StjOptions.WriterOptions.Encoder,

                NumberHandling = JsonNumberHandling.Strict,

                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = false,

                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                DictionaryKeyPolicy = null,
                ReferenceHandler = null,
            };

        protected override ICollection<JsonConverter> GetConvertersCore()
        {
            return new JsonConverter[]
            {
                new DoubleJsonConverter(),
                new StringJsonConverter(),

                // TODO(dmitry.azaraev): Serializing/Deserializing Unit type is should not be needed, but need do some tests before removal.
                new UnitJsonConverter()
            };
        }
    }
}
