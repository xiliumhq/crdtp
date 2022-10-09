using System;
using System.Diagnostics.CodeAnalysis;

namespace Xilium.Crdtp.Client;

public readonly struct CrdtpResponse<T>
{
    // Used by CrdtpResponse::.ctor(CrdtpResponse<Unit>).
    internal readonly CrdtpErrorResponse? _error;
    private readonly T _result;

    public CrdtpResponse(CrdtpErrorResponse error)
    {
        Check.Argument.NotNull(error, nameof(error));
        _error = error;
        _result = default!;
    }

    public CrdtpResponse(T result)
    {
        _error = null;
        _result = result;
    }

    // Used by CrdtpResponse::AsGeneric()
    internal CrdtpResponse(CrdtpErrorResponse? error, T result)
    {
        _error = error;
        _result = result;
    }

    public bool IsSuccess => _error == null;

    public bool IsError => _error != null;

    public CrdtpErrorResponse GetError()
    {
        if (_error != null) return _error;

        // TODO: Use ThrowHelper to make call sites smaller
        throw new InvalidOperationException("CrdtpErrorResponse doesn't represents error.");
    }

    public bool TryGetError([NotNullWhen(true)] out CrdtpErrorResponse? error)
    {
        return (error = _error) != null;
    }

    // TODO: [API] Naming: CrdtpResponse::GetResult() - should it be just `Result` property?
    // TODO: [API] Naming: CrdtpResponse::GetResult() - should it be named differently?
    public T GetResult()
    {
        if (_error != null) CrdtpErrorResponseException.Throw(_error);
        return _result;
    }

    // TODO: [API] [CrdtpResponse::TryGetResult] There is no similar method in
    // CrdtpResponse, so it is probably bad idea to make such method. Alternatively
    // CrdtpResponse might have TryGetResult(Unit out result), so it might be
    // at least duck-typed.
    public bool TryGetResult([NotNullWhen(true)] out T? result)
    {
        result = _result;
        return _error == null;
    }
}
