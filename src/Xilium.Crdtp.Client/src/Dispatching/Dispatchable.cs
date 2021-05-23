using System;

namespace Xilium.Crdtp.Client.Dispatching
{
    // TODO(dmitry.azaraev): Dispatchable - can be simplified,
    // PayloadType is not needed: this type might hold one of
    // 1. method result
    // 2. event parameters
    // 3. error (method only)
    // So there is just enough bool IsProtocolError.
    internal readonly ref struct Dispatchable
    {
        private readonly string? _sessionId;
        private readonly int? _callId;
        private readonly string? _method;
        private readonly PayloadType _dataType;
        private readonly ReadOnlySpan<byte> _data;

        public Dispatchable(string? sessionId, int? callId, string? method,
            PayloadType dataType,
            ReadOnlySpan<byte> data)
        {
            _sessionId = sessionId;
            _callId = callId;
            _method = method;
            _dataType = dataType;
            _data = data;
        }

        public string? SessionId => _sessionId;
        public int? CallId => _callId;
        public string? Method => _method;
        public PayloadType DataType => _dataType;
        public ReadOnlySpan<byte> Data => _data;

        public enum PayloadType
        {
            None = 0,
            Result = 1,
            Error = 2,
            Params = 3,
        }
    }
}
