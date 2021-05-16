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

            public override void OnConnect()
            {
                // Console.WriteLine("CrdtpClient::ConnectionDelegate::OnConnect");
            }

            public override void OnClose()
            {
                // Console.WriteLine("CrdtpClient::ConnectionDelegate::OnClose");

                // TODO(dmitry.azaraev): (High) Handle close logic, e.g. detach all sessions,
                // which should indirectly detach all sessions / cancel all pending calls

                // TODO(dmitry.azaraev): Also emit events.
            }

            public override void OnMessage(ReadOnlySpan<byte> message)
            {
                // Console.WriteLine("CrdtpClient::ConnectionDelegate::OnMessageCore:");

                if (CrdtpFeatures.IsLogRecvEnabled)
                {
                    // TODO(dmitry.azaraev): (High) Needs API for protocol logging (receiving).
#if NET5_0_OR_GREATER
                    var text = System.Text.Encoding.UTF8.GetString(message);
#else
                    var text = System.Text.Encoding.UTF8.GetString(message.ToArray());
#endif
                    Console.WriteLine("< {0}", text);
                }

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
        }
    }
}
