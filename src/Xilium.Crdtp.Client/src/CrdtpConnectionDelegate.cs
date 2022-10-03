using System;
using System.Collections.Generic;

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

        // TODO: Make helper to produce AggregateException
        internal void OnAbort(List<Exception>? exceptions)
        {
            Exception? exception;
            if (exceptions != null && exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    exception = exceptions[0];
                }
                else
                {
                    exception = new AggregateException(exceptions);
                }
            }
            else
            {
                exception = null;
            }

            OnAbort(exception);
        }
    }
}
