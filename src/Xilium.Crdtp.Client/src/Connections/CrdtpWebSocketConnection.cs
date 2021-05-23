// TODO(dmitry.azaraev): Review supported platforms. NET5 - clearly works, but rest platforms might work differently.
// TODO(dmitry.azaraev): (High) CrdtpWebSocketConnection::CloseAsync: When synchronization required - both SendAsync & CloseAsync should be synchronized.
// TODO: reorganize this code. At top level this should depends which interface we should use (with value-based results)
// second, is regardless of interface, underlying implementation might or might not require synchronization.
// Current assumption what presense of value-based results doesn't require synchronization is might be wrong.
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
#define HasValueSendAsync
#define HasValueReceiveAsync
#else
#define WS_REQUIRE_SYNCHRONIZATION
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

#if WS_REQUIRE_SYNCHRONIZATION
        private readonly SemaphoreSlim _sendAsyncLock = new SemaphoreSlim(1, 1);
#endif

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

        protected override void DisposeCore()
        {
            _socket.Dispose();
        }

        protected override Task OpenAsyncCore(CancellationToken cancellationToken)
            => _socket.ConnectAsync(_endpoint, cancellationToken);

        protected override Task CloseAsyncCore(CancellationToken cancellationToken)
            => _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);

        protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
        {
#if HasValueSendAsync
            return _socket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken: default);
#else
            if (MemoryMarshal.TryGetArray(message, out var arraySegment))
            {
#if WS_REQUIRE_SYNCHRONIZATION
                if (_sendAsyncLock.Wait(0, default))
                {
                    return SendLockAcquiredAsync(arraySegment);
                }
                else
                {
                    return SendFallbackAsync(arraySegment);
                }
#else
                var result = _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken: default);
                return new ValueTask(result);
#endif
            }
            else throw Error.InvalidOperation("Failed to get underlying array.");
#endif
        }

#if WS_REQUIRE_SYNCHRONIZATION
        private ValueTask SendLockAcquiredAsync(ArraySegment<byte> arraySegment)
        {
            Task? sendTask = null;
            var releaseSemaphore = true;
            try
            {
                sendTask = _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken: default);

                if (sendTask.IsCompleted)
                {
                    return new ValueTask(sendTask);
                }

                releaseSemaphore = false;
            }
            finally
            {
                if (releaseSemaphore)
                {
                    _sendAsyncLock.Release();
                }
            }

            return WaitForSendTaskAsync(sendTask);
        }

        private async ValueTask WaitForSendTaskAsync(Task writeTask)
        {
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            // TODO: Exceptions should be handled at client side.
            //catch (Exception exc) when (!(exc is OperationCanceledException))
            //{
            //    throw _state == WebSocketState.Aborted ?
            //        CreateOperationCanceledException(exc) :
            //        new WebSocketException(WebSocketError.ConnectionClosedPrematurely, exc);
            //}
            finally
            {
                _sendAsyncLock.Release();
            }
        }

        private async ValueTask SendFallbackAsync(ArraySegment<byte> arraySegment)
        {
            await _sendAsyncLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken: default);
            }
            finally
            {
                _sendAsyncLock.Release();
            }
        }
#endif

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
                        // Note that chromium doesn't close connection gracefully (doesn't send close frame),
                        // So this case can be tested only own connection.
                        //if (receiveResult.MessageType == WebSocketMessageType.Close)
                        //{
                        //    // Handle gracefully, e.g. it is successfully closed.
                        //    break;
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
