using System;

namespace Xilium.Crdtp.Client
{
    /// <summary>
    /// Exception thrown for an error response with <see cref="CrdtpErrorResponse"/> information.
    /// </summary>
    /// <remarks>
    /// This exception intentionally derived from <see cref="Exception"/> to easily distinct
    /// error respose from other infrastructure exceptions.
    /// </remarks>
    public sealed class CrdtpErrorResponseException : Exception
    {
        private readonly CrdtpErrorResponse _errorResponse;

        public CrdtpErrorResponseException(CrdtpErrorResponse errorResponse)
            : base(errorResponse.Message)
        {
            _errorResponse = errorResponse;
        }

        public CrdtpErrorResponse ErrorResponse => _errorResponse;
    }
}
