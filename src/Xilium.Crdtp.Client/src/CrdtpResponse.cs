using System;
using System.Diagnostics.CodeAnalysis;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client;

/// <summary>
/// Represents response with empty (void) result.
/// </summary>
/// <remarks>
/// Internally, all requests uses <c>CrdtpResponse<Unit></c>, and then it can
/// be converted to this type by using <see cref="CrdtpResponse.CrdtpResponse(Xilium.Crdtp.Client.CrdtpResponse{Unit})"/>
/// constructor.
/// </remarks>
public readonly struct CrdtpResponse
{
    private readonly CrdtpErrorResponse? _error;

    public CrdtpResponse(CrdtpErrorResponse error)
    {
        Check.Argument.NotNull(error, nameof(error));
        _error = error;
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public CrdtpResponse(Unit result)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _error = null;
    }

    // TODO: Add extension method to convert CrdtpResponse<Unit> -> CrdtpResponse, and avoid ctor?
    // However this ctor is useful for generated code.
    public CrdtpResponse(CrdtpResponse<Unit> response)
    {
        _error = response._error;
    }

    public bool IsSuccess => _error == null;

    public bool IsError => _error != null;

    public bool TryGetError([NotNullWhen(true)] out CrdtpErrorResponse? error)
    {
        return (error = _error) != null;
    }

    public CrdtpErrorResponse GetError()
    {
        if (_error != null) return _error;

        // TODO: Use ThrowHelper to make call sites smaller
        throw new InvalidOperationException("CrdtpErrorResponse doesn't represents error.");
    }

    public void GetResult()
    {
        if (_error != null) CrdtpErrorResponseException.Throw(_error);
    }

    //public bool TryGetResult()
    //{
    //    return _error == null;
    //}

    // TODO: Naming: AsGeneric, AsUnit?
    public CrdtpResponse<Unit> AsGeneric()
        => new CrdtpResponse<Unit>(_error, default(Unit)!);
}
