// TODO(dmitry.azaraev): Review supported platforms.
#if NETCOREAPP3_0_OR_GREATER
#define HasValueSendAsync
#define HasValueReceiveAsync
#else
#define NeedsInteropServices
#endif

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Buffers;

#if NeedsInteropServices
using System.Runtime.InteropServices;
#endif

namespace Xilium.Crdtp.Client
{
    public sealed class CrdtpWebSocketConnection : CrdtpConnection
    {
        private readonly ClientWebSocket _socket;
        private readonly Uri _endpoint;

        public CrdtpWebSocketConnection(CrdtpConnectionDelegate handler, string url)
            : this(handler, new Uri(url))
        { }

        public CrdtpWebSocketConnection(CrdtpConnectionDelegate handler, Uri url)
            : base(handler)
        {
            _endpoint = url;
            _socket = new ClientWebSocket();
            _socket.Options.KeepAliveInterval = TimeSpan.Zero;
            // _socket.Options.SetBuffer(64 * 1024, 64 * 1024);
        }

        protected override Task OpenAsyncCore(CancellationToken cancellationToken)
            => _socket.ConnectAsync(_endpoint, cancellationToken);

        protected override Task CloseAsyncCore(CancellationToken cancellationToken)
            => _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);

        protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
#if HasValueSendAsync
            return _socket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
#else
            if (MemoryMarshal.TryGetArray(message, out var arraySegment))
            {
                var result = _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken);
                return new ValueTask(result);
            }
            else throw Error.InvalidOperation("Failed to get underlying array.");
#endif
        }

        protected override CrdtpConnectionReader? CreateReader()
            => new Reader(this);

        private sealed class Reader : CrdtpConnectionReader
        {
            private readonly CrdtpWebSocketConnection _connection;

            public Reader(CrdtpWebSocketConnection connection)
            {
                _connection = connection;
            }

            protected override async Task RunReadLoopAsync(CancellationToken cancellationToken)
            {
                var socket = _connection._socket;
                var handler = _connection.Delegate;

                // TODO(dmitry.azaraev): CrdtpWebSocketConnection - use buffer pool.

                var messageBuffer = new CrdtpArrayBufferWriter<byte>(4096);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Read message into buffer.
                    try
                    {
#if HasValueReceiveAsync
                        ValueWebSocketReceiveResult
#else
                        WebSocketReceiveResult
#endif
                            receiveResult;

                        do
                        {
                            var targetBuffer = messageBuffer
#if HasValueReceiveAsync
                                .GetMemory();
#else
                                .GetArraySegment();
#endif

                            // CancellationToken is not passed intentionally: because ReceiveAsync will throw on socket closing attempt.
                            receiveResult = await socket
                                .ReceiveAsync(targetBuffer, cancellationToken: default)
                                .ConfigureAwait(false);

                            Check.That(receiveResult.MessageType == WebSocketMessageType.Text, "Unexpected message type received.");

                            messageBuffer.Advance(receiveResult.Count);
                        }
                        while (!receiveResult.EndOfMessage);

                        // TODO(dmitry.azaraev): (Low) Handle graceful closing.
                        // Note that chromium doesn't close connection gracefully (doesn't send close frame).
                        //if (receiveResult.MessageType == WebSocketMessageType.Close)
                        //{
                        //    // handle gracefully, e.g. it is successfully closed.
                        //}
                    }
                    catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        if (_connection.State >= CrdtpConnectionState.Closing)
                        {
                            return;
                        }

                        throw;
                    }

                    // TODO(dmitry.azaraev): (?) Some method results might be processed asynchronously (for example screenshot result).
                    // So there is would be nice option to pass buffer, which we might keep for self. Delay decision on this, as there
                    // is not enough information about preffered way. Also require marking such methods in generator / extending API, etc.
                    handler.OnMessage(messageBuffer.WrittenMemory);
                    messageBuffer.Clear();
                }
            }
        }
    }
}
