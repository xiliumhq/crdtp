using System;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpConnectionDelegate
    {
        public abstract void OnConnect();
        public abstract void OnClose();

        public abstract void OnMessage(ReadOnlySpan<byte> message);
        public void OnMessage(ReadOnlyMemory<byte> message) => OnMessage(message.Span);
    }
}
