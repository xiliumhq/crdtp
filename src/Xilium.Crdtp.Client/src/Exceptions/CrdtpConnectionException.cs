using System;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): Redesign exceptions. Also add CrdtpException?
    public sealed class CrdtpConnectionException : Exception
    {
        public CrdtpConnectionException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }
}
