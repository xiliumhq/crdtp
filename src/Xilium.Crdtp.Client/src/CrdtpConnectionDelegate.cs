using System;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpConnectionDelegate
    {
        public abstract void OnOpen();
        public abstract void OnClose();
        public abstract void OnAbort(Exception? exception);

        public abstract void OnMessage(ReadOnlySpan<byte> message);

        /// <remarks>
        /// This method should never throw exception. In order if unexpection
        /// exception occur implementation of this method, must log exception or
        /// abort CrdtpClient.
        /// </remarks>
        public void OnMessage(ReadOnlyMemory<byte> message) => OnMessage(message.Span);
    }
}
