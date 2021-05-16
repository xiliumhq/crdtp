using System;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): Redesign exceptions. Also add CrdtpException.
    public sealed class CrdtpErrorException : Exception
    {
        private readonly CrdtpError _error;

        public CrdtpErrorException(CrdtpError error)
            : base(error.Message)
        {
            _error = error;
        }

        public CrdtpError Error => _error;
    }
}
