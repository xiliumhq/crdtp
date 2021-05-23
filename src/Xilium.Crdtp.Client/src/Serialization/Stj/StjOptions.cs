using System.Text.Encodings.Web;
using System.Text.Json;

namespace Xilium.Crdtp.Client.Serialization
{
    internal static class StjOptions
    {
        // See crdtp/cbor.cc, crdtp/json.cc, kStackLimit = 300:
        // When parsing CBOR, we limit recursion depth for objects and arrays
        // to this constant.
        public const int MaxDepth = 300;

        public static readonly JavaScriptEncoder? Encoder = null;

        public static readonly JsonReaderOptions ReaderOptions = new JsonReaderOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = MaxDepth,
        };

        public static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions
        {
            Encoder = Encoder,
            Indented = false,
#if DEBUG
            SkipValidation = false,
#else
            SkipValidation = true,
#endif
        };
    }
}
