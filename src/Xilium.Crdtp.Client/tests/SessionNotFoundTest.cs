using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;
using Xunit;

namespace Xilium.Crdtp.Client.Tests;

// Check handling of server-side SessionNotFound error.
// See `DispatchResponse::SessionNotFound` (https://source.chromium.org/chromium/chromium/src/+/main:third_party/inspector_protocol/crdtp/dispatch.h;drc=dd69cb0b05666656d3bb857a6b000f984430643e;l=82)
// This error are responded without sessionId property, so, client should
// be ready to such cases.
// See also https://github.com/xiliumhq/crdtp/issues/13
public class SessionNotFoundTest
{
    internal sealed class TestConnection : CrdtpConnection
    {
        public TestConnection(CrdtpConnectionDelegate @delegate)
            : base(@delegate)
        { }

        protected override void DisposeCore() { }

        protected override CrdtpConnectionReader? CreateReader() => null;

        protected override Task OpenAsyncCore(CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override Task CloseAsyncCore(CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
        {
            // TODO: Implement generic crdtp-server to make tests easier

            var dispatchable = DispatchableParser.Parse(message.Span);

            if (dispatchable.Method == "Connectivity2.echo")
            {
                var response = new CrdtpMessage
                {
                    Id = dispatchable.CallId!.Value,
                    Result = new object(),
                    // Error = new CrdtpErrorResponse(-1, "Hello, world!"),
                    SessionId = dispatchable.SessionId,
                };
                var jsonPayload = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    AllowTrailingCommas = false, // StjOptions.ReaderOptions.AllowTrailingCommas,
                    ReadCommentHandling = JsonCommentHandling.Disallow, // StjOptions.ReaderOptions.CommentHandling,
                    MaxDepth = 300, // StjOptions.ReaderOptions.MaxDepth,

                    WriteIndented = false, // StjOptions.WriterOptions.Indented,
                    // Encoder = StjOptions.WriterOptions.Encoder,

                    NumberHandling = JsonNumberHandling.Strict,

                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = false,

                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                    DictionaryKeyPolicy = null,
                    ReferenceHandler = null,
                });
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                Delegate.OnMessage(bytes.AsMemory());
                return ValueTask.CompletedTask;
            }
            else if (dispatchable.Method == "Connectivity2.sessionNotFound")
            {
                var response = new CrdtpMessage
                {
                    Id = dispatchable.CallId!.Value,
                    Error = new CrdtpErrorResponse(-32001, "Session with given id not found."),
                    // SessionId is not included by server.
                    // TODO: Consider to test also case when SessionId is included.
                    // SessionId = dispatchable.SessionId,
                };
                var jsonPayload = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    AllowTrailingCommas = false, // StjOptions.ReaderOptions.AllowTrailingCommas,
                    ReadCommentHandling = JsonCommentHandling.Disallow, // StjOptions.ReaderOptions.CommentHandling,
                    MaxDepth = 300, // StjOptions.ReaderOptions.MaxDepth,

                    WriteIndented = false, // StjOptions.WriterOptions.Indented,
                    // Encoder = StjOptions.WriterOptions.Encoder,

                    NumberHandling = JsonNumberHandling.Strict,

                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = false,

                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                    DictionaryKeyPolicy = null,
                    ReferenceHandler = null,
                });
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                Delegate.OnMessage(bytes.AsMemory());
                return ValueTask.CompletedTask;
            }
            else
            {
                throw new InvalidOperationException("Unknown method.");
            }
        }

        private struct CrdtpMessage
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("result")]
            public object? Result { get; set; }

            [JsonPropertyName("error")]
            public CrdtpErrorResponse? Error { get; set; }

            [JsonPropertyName("sessionId")]
            public string? SessionId { get; set; }
        }
    }

    [Fact]
    public async Task Run()
    {
        using var client = new CrdtpClient((handler) => new TestConnection(handler));

        var defaultSession = new CrdtpSession(client, "");
        client.Attach(defaultSession);

        await client.OpenAsync();

        using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var response = await defaultSession.SendCommandAsync("Connectivity2.echo", new EmptyRequest(),
            cancellationToken: tcs.Token);
        Assert.True(response.IsSuccess);

        var session2 = new CrdtpSession(client, "some-session-id");
        client.Attach(session2);
        var response2 = await session2.SendCommandAsync("Connectivity2.sessionNotFound",
            new EmptyRequest(), cancellationToken: tcs.Token);
        Assert.True(response2.IsError);
        var error = response2.GetError();
        Assert.Equal(-32001, error.Code);
        Assert.Equal("Session with given id not found.", error.Message);
    }

    private sealed class EmptyRequest { }
    private sealed class EmptyResponse { }
}
