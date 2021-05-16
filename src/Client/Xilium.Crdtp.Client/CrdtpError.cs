using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): Make CrdtpError type free from STJ dependency, and use Converter or explicit parser to read it.
    public sealed class CrdtpError
    {
        public CrdtpError(int code, string message, string? data)
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
    }
}
