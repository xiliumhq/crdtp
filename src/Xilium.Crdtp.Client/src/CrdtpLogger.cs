using System;

namespace Xilium.Crdtp.Client
{
    // TODO: This can be changed to be pure interface, but it might hold
    // some helpers to perform transcoding, so probably interface not best way
    public abstract class CrdtpLogger
    {
        public abstract void LogSend(ReadOnlySpan<byte> message);
        public abstract void LogReceive(ReadOnlySpan<byte> message);
    }
}
