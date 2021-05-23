using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;

namespace Xilium.Crdtp.Client.Api
{
    // TODO: Remove API interfaces when they are no more needed.

    internal interface ISessionApi
    {
        string? SessionId { get; }

        bool IsAttached { get; }

        CrdtpClient GetClient();
        // CrdtpClient? TryGetClient(out CrdtpClient? client);

        void CancelPendingRequests();

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

        void AddEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default);
        void AddEventHandler(string name, EventHandler handler, object? sender = default);
        bool RemoveEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default);
        bool RemoveEventHandler(string name, EventHandler handler, object? sender = default);

        #endregion

        // Infrastructure methods.
        int GetNextCallId();
        void UseSerializerOptions(StjSerializerOptions options);
    }
}
