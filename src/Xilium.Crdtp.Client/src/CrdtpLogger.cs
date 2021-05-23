using System;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpLogger
    {
        public abstract void LogSend(ReadOnlySpan<byte> message);
        public abstract void LogReceive(ReadOnlySpan<byte> message);
    }
}
