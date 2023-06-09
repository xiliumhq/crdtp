#if XI_CRDTP_USE_INTERNAL_API_INTERFACES
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Threading;

namespace Xilium.Crdtp.Client;

internal interface ISessionApi
{
    string? SessionId { get; }

    bool IsAttached { get; }

    CrdtpClient GetClient();
    // CrdtpClient? TryGetClient(out CrdtpClient? client);

    void CancelPendingRequests();
    // TODO: Add CancelPendingRequests with request filter

    #region Commands

    Task ExecuteCommandAsync<TRequest>(
        string method,
        TRequest parameters,
        CancellationToken cancellationToken = default);
    Task ExecuteCommandAsync<TRequest>(
        JsonEncodedText method,
        TRequest parameters,
        CancellationToken cancellationToken = default);

    Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
        string method,
        TRequest parameters,
        CancellationToken cancellationToken = default);
    Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
        JsonEncodedText method,
        TRequest parameters,
        CancellationToken cancellationToken = default);

    Task<CrdtpResponse<TResponse>> SendCommandAsync<TRequest, TResponse>(
        string method,
        TRequest parameters,
        CancellationToken cancellationToken = default);
    Task<CrdtpResponse<TResponse>> SendCommandAsync<TRequest, TResponse>(
        JsonEncodedText method,
        TRequest parameters,
        CancellationToken cancellationToken = default);

    #endregion

    #region Events

    // TODO: Introduce new AddEventListener method set.
    // TODO: Add option "Once"
    // TODO: Add other overloads.
    // TODO: If profitable (and it should - use JsonEncodedText) as overloads.
    void AddEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default, TaskRunner? taskRunner = default);
    void AddEventHandler(string name, EventHandler handler, object? sender = default, TaskRunner? taskRunner = default);
    bool RemoveEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default);
    bool RemoveEventHandler(string name, EventHandler handler, object? sender = default);

    #endregion

    // Infrastructure methods.
    void UseSerializerOptions(StjSerializerOptions options);
}
#endif
