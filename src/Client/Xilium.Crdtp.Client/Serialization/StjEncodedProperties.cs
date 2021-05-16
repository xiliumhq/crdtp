using System.Text.Json;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Low) Use module initializer for StjEncodedProperties.

    internal static class StjEncodedProperties
    {
        public static readonly JsonEncodedText Id = JsonEncodedText.Encode("id");
        public static readonly JsonEncodedText Method = JsonEncodedText.Encode("method");
        public static readonly JsonEncodedText Params = JsonEncodedText.Encode("params");
        public static readonly JsonEncodedText SessionId = JsonEncodedText.Encode("sessionId");
    }
}
