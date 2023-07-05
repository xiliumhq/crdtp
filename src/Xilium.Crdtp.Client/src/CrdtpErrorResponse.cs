using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Low) Make CrdtpError type free from STJ dependency, and use Converter or explicit parser to read it.
    // TODO: Make instances of this class to be immutable.
    public sealed class CrdtpErrorResponse
    {
        // This class used for deserialization.
        [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CrdtpErrorResponse() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public CrdtpErrorResponse(int code, string message, string? data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        public CrdtpErrorResponseException ToException()
            => new CrdtpErrorResponseException(this);
    }
}
