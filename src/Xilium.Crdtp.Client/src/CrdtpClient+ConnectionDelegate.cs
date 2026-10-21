using System;
using System.Buffers;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Threading;

namespace Xilium.Crdtp.Client
{
    partial class CrdtpClient
    {
        internal sealed class ConnectionDelegate : CrdtpConnectionDelegate
        {
            private readonly CrdtpClient _client;

            public ConnectionDelegate(CrdtpClient client)
            {
                _client = client;
            }

            public override void OnOpen()
            {
                _client.OnOpen();
            }

            public override void OnClose()
            {
                _client.OnClose();
            }

            public override void OnAbort(Exception? exception)
            {
                _client.OnAbort(exception);
            }

            public override void OnMessage(ReadOnlySpan<byte> message)
            {
                var taskRunner = _client._taskRunner;
                if (taskRunner == null)
                {
                    OnMessageCore(message);
                }
                else
                {
                    // TODO: This method simply copies message, to own buffer,
                    // but this is not very efficient. It should be better if reader itself
                    // will provide object with buffer or portion of buffer, which then can be
                    // reused. Reusing strategy is not simple: if we share multiple messages
                    // in same buffer, then we should track it's uses. There is need some metrics,
                    // to choose better/simplier strategy. Generally It should aim:
                    // - smallest number of objects/buffers across process (e.g. better pool them process-wide);
                    // - read messages as much as possible from server in queue, so
                    //   server will be able to free own queue (need some balance here: let browser
                    //   maintain own queue is also good, but after browser reaches limit - it will die);
                    var messageLength = message.Length;
                    // TODO: This is TLS pool, but used between threads - it thrashed, sorry.
                    var pooledBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
                    message.CopyTo(pooledBuffer);
                    // TODO: Avoid closure.
                    taskRunner.PostTask(() =>
                    {
                        OnMessageCore(pooledBuffer.AsSpan().Slice(0, messageLength));
                        ArrayPool<byte>.Shared.Return(pooledBuffer);
                    });
                }
            }

            private void OnMessageCore(ReadOnlySpan<byte> message)
            {
                try
                {
                    _client._logger?.LogReceive(message);
#if DEBUG
                    if (_client._taskRunner is { } taskRunner)
                    {
                        DebugCheck.That(ReferenceEquals(taskRunner, TaskRunner.Current));
                    }
#endif

                    var isCborMessage = CborHelper.IsCborMessage(message);

                    Check.That(isCborMessage == (_client.Encoding == CrdtpEncoding.Cbor), "Unexpected message encoding.");

                    if (isCborMessage)
                    {
                        throw Error.NotSupported("CBOR protocol mode is not supported yet.");
                    }

                    var dispatchable = DispatchableParser.Parse(message);
                    _client.Dispatch(dispatchable);
                }
                catch (Exception ex)
                {
                    // TODO: always specify (e.g. default handler) in CrdtpClient ctor,
                    // and assume what it is always present. By default it should
                    // return false (meaning exception is unhandled) and abort client.
                    var handler = _client._handler;
                    if (handler == null || !handler.OnUnhandledException(ex))
                    {
                        _client.Abort(ex);
                    }
                }
            }
        }
    }
}
