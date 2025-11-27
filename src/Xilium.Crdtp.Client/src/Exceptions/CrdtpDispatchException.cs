using System;

namespace Xilium.Crdtp.Client.Exceptions;

public sealed class CrdtpDispatchException : Exception
{
    private readonly string _sessionId;
    private readonly int? _callId;
    private readonly string? _method;

    public CrdtpDispatchException(string? message, string sessionId, int? callId, string? method)
        : base(message)
    {
        _sessionId = sessionId;
        _callId = callId;
        _method = method;
    }

    public string SessionId => _sessionId;
    public int? CallId => _callId;
    public string? Method => _method;
}
