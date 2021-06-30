using System;
using Xilium.Crdtp.Client.Serialization;

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
                try
                {
                    _client._logger?.LogReceive(message);

                    var isCborMessage = CborHelper.IsCborMessage(message);

                    Check.That(isCborMessage == (_client.Encoding == CrdtpEncoding.Cbor), "Unexpected message encoding.");

                    if (isCborMessage)
                    {
                        throw Error.NotSupported("CBOR protocol mode is not supported yet.");
                    }

                    var dispatchable = DispatchableParser.Parse(message);

                    if (!_client.TryGetSession(dispatchable.SessionId, out var session))
                    {
                        throw Error.InvalidOperation("Session not found.");
                    }

                    session.Dispatch(dispatchable);
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
