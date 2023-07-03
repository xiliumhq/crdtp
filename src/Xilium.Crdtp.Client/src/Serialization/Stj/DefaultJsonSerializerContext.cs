using System.Text.Json;
using System.Text.Json.Serialization;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client.Serialization;

[JsonSerializable(typeof(CrdtpErrorResponse))]
[JsonSerializable(typeof(Unit))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class DefaultJsonSerializerContext : JsonSerializerContext
{
    public static DefaultJsonSerializerContext Create()
        => new DefaultJsonSerializerContext(CreateJsonSerializerOptions());

    internal static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
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

        return options;
    }
}
