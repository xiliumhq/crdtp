using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Buffers;
using Xilium.Crdtp.Client.Dispatching;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Crdtp.Core;
using Xilium.Threading;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (CrdtpSession::RequestMap) Consider to use specialized request map
    // (better do it as value type?), which will be optimized to common case
    // (e.g. where only few concurrent requests are exist).
    // TODO(dmitry.azaraev): (CrdtpSession) Consider to use more simplier request queue, HashSet/Dictionary+lock is generally better,
    // or lock-free table, or some different. There is almost zero concurrency for single session.

    public sealed class CrdtpSession
#if XI_CRDTP_USE_INTERNAL_API_INTERFACES
        : ISessionApi
#endif
    {
        internal readonly CrdtpClient _client;
        private readonly string _sessionId;
        private readonly CrdtpSessionHandler _handler;
        internal readonly TaskRunner? _taskRunner;

        // TODO: Rename to StateLock
        internal readonly object StateAndRequestMapLock = new object();
        private bool _isAttached;

        internal object EventDispatcherMapLock => _eventDispatchers;
        private readonly Dictionary<string, CrdtpDispatcher> _eventDispatchers;

        private readonly JsonEncodedText _jsonEncodedSessionId;

        // This field accessed by CrdtpClient when request added/removed from
        // request map, and this operations performed under RequestMapLock;
        internal int _numberOfPendingRequests;

        public CrdtpSession(CrdtpClient client, string sessionId,
            CrdtpSessionHandler? handler = null,
            TaskRunner? taskRunner = null)
        {
            Check.Argument.NotNull(client, nameof(client));
            Check.Argument.NotNull(sessionId, nameof(sessionId));

            _client = client;
            _sessionId = sessionId;
            _handler = handler ?? DefaultSessionHandler.Instance;
            _taskRunner = taskRunner ?? _client._taskRunner;
            _jsonEncodedSessionId = JsonEncodedText.Encode(sessionId);
            _eventDispatchers = new Dictionary<string, CrdtpDispatcher>();
        }

        public string SessionId => _sessionId;

        public TaskRunner? TaskRunner => _taskRunner;

        public bool IsAttached => _isAttached;

        public CrdtpClient GetClient() => _client;

        #region Attach & Detach

        internal void AttachToInternal(CrdtpClient client)
        {
            // Both locks must be acquired to prevent notification dispatchers
            // be called before session completes state transition.
            DebugCheck.That(Monitor.IsEntered(StateAndRequestMapLock), $"{nameof(StateAndRequestMapLock)} should be held.");
            DebugCheck.That(Monitor.IsEntered(EventDispatcherMapLock), $"{nameof(EventDispatcherMapLock)} should be held.");

            if (!_isAttached)
            {
                _isAttached = true;
                _handler.OnAttach();
            }
        }

        internal void DetachInternal()
        {
            DebugCheck.That(Monitor.IsEntered(StateAndRequestMapLock), $"{nameof(StateAndRequestMapLock)} should be held.");

            if (_isAttached)
            {
                _isAttached = false;
                // TODO: Cancel pending requests with reason
                CancelPendingRequests(); // TODO: this can be called outside of the lock, or from Attach/Detach methods
                _handler.OnDetach();
            }
        }

        public void CancelPendingRequests()
            => _client.CancelPendingRequests(this);

        #endregion

        internal StjTypeInfoResolver StjTypeInfoResolver
            => CrdtpClient.StjTypeInfoResolver;

        public void UseSerializationContextFactory(StjSerializationContextFactory options)
            => CrdtpClient.StjTypeInfoResolver.Add(options);

        #region Commands

        public async Task ExecuteCommandAsync<TRequest>(
            string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(
                method, parameters, cancellationToken);
            _ = response.GetResult();
        }

        public async Task ExecuteCommandAsync<TRequest>(
            JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(
                method, parameters, cancellationToken);
            _ = response.GetResult();
        }

        public async Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
            string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, TResponse>(method,
                parameters, cancellationToken);
            return response.GetResult();
        }

        public async Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
            JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, TResponse>(method,
                parameters, cancellationToken);
            return response.GetResult();
        }

        // TODO(dmitry.azaraev): (Low) Add SendCommandAsync<TRequest> which might return CrdtpResponse?

        // TODO: Generic sending method for sending command(s).
        // This method is not necessary might be accessible, if client is built without Json/Dynamic support.
        // [DynamicallyAccessedMembers(MembersAccessedOnWrite)] => public fields & public properties

        public async Task<CrdtpResponse> SendCommandAsync<TRequest>(string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(
                JsonEncodedText.Encode(method), parameters, cancellationToken);
            return new CrdtpResponse(response);
        }

        public async Task<CrdtpResponse> SendCommandAsync<TRequest>(JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(method,
                parameters, cancellationToken);
            return new CrdtpResponse(response);
        }

        public Task<CrdtpResponse<TResponse>> SendCommandAsync<TRequest, TResponse>(
            string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            // TODO(dmitry.azaraev): Cache string -> JsonEncodedText in CrdtpClient, but need some measures.
            return SendCommandAsync<TRequest, TResponse>(
                JsonEncodedText.Encode(method),
                parameters,
                cancellationToken);
        }

        public Task<CrdtpResponse<TResponse>> SendCommandAsync<TRequest, TResponse>(
            JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Debugger.NotifyOfCrossThreadDependency();
            // TODO: Enable checks above when tests be more robust.

            // TODO: ThrowIfNotAttached() is not used here, because we capture
            // client reference.
            var client = _client;
            if (client == null) ThrowNotAttached();

            Check.That(client.Encoding == CrdtpEncoding.Json);

            // Validate command before send
            {
                var maybeError = _handler.OnBeforeSend(method);
                if (maybeError != null)
                {
                    return Task.FromResult(new CrdtpResponse<TResponse>(maybeError));
                }
            }

            var callId = client.GetNextCallId();
            var bufferWriter = SerializeRequest(callId, method, parameters);
            var shouldReturnBufferWriter = true;
            try
            {
                // now we can send buffer and release it, but before we should register request
                var request = new CrdtpRequest<TResponse>(this, callId); // cancellationToken);

                lock (StateAndRequestMapLock)
                {
                    ThrowIfNotAttached();
                    client.AddRequest(callId, request);
                }

                // TODO(dmitry.azaraev): ?

                // TODO(dmitry.azaraev): (High) Handle cancellation, e.g. once request is created and registered,
                // if send is fail or send, request should get somehow know about this situation.

                DebugCheck.That(!request.Task.IsCompleted);

                ValueTask sendTask;
                try
                {
                    sendTask = client.SendAsync(bufferWriter.WrittenMemory);
                }
                catch (CrdtpConnectionException ccex)
                {
                    // TODO: (HIGH) this should abort both - connection and request
                    request.Abort(ccex);
                    throw;
                }
                catch (Exception ex)
                {
                    var ccex = new CrdtpConnectionException("Failed to send request.", ex);
                    // TODO: (HIGH) this should abort both - connection and request
                    request.Abort(ex);
                    throw ccex;
                }

                // Register cancellation token lately (after SendAsync call returns back),
                // allowing synchronous connections handle any number response(s)
                // without registering request for cancellation, because
                // in that case we already might have response in the hands.
                request.RegisterForCancellation(cancellationToken);

                if (sendTask.IsCompletedSuccessfully)
                {
                    return request.Task;
                }

                shouldReturnBufferWriter = false;
                return SendCommandAsyncInternal(sendTask, bufferWriter, request);
            }
            finally
            {
                if (shouldReturnBufferWriter)
                {
                    Pools.BufferWriterPool.Return(bufferWriter);
                }
            }
        }

        private async Task<CrdtpResponse<TResponse>> SendCommandAsyncInternal<TResponse>(
            ValueTask sendTask,
            CrdtpArrayBufferWriter<byte> bufferWriter,
            CrdtpRequest<TResponse> request)
        {
            // TODO: Reimplement this, as we need await only sending, while
            // we may return request task without awaiting on it.

            try
            {
                await sendTask.ConfigureAwait(false);
            }
            catch (CrdtpConnectionException ccex)
            {
                // TODO: (HIGH) this should abort both - connection and client
                request.Abort(ccex);
            }
            catch (Exception ex)
            {
                // TODO: (HIGH) this should abort both - connection and client
                var ccex = new CrdtpConnectionException("Failed to send request.", ex);
                request.Abort(ccex);
            }
            finally
            {
                Pools.BufferWriterPool.Return(bufferWriter);
            }

            return await request.Task.ConfigureAwait(false);
        }

        private CrdtpArrayBufferWriter<byte> SerializeRequest<TRequest>(int callId, JsonEncodedText method, TRequest parameters)
        {
            var bufferWriter = Pools.BufferWriterPool.Rent();
            var shouldReturnBufferWriter = true;
            DebugCheck.That(bufferWriter.WrittenCount == 0);
            try
            {
                var encoder = Pools.Utf8JsonWriterPool.Rent();
                try
                {
                    encoder.Reset(bufferWriter);

                    encoder.WriteStartObject();
                    encoder.WriteNumber(StjEncodedProperties.Id, callId);
                    encoder.WriteString(StjEncodedProperties.Method, method);

                    // TODO(dmitry.azaraev): (Undecided) For convenience there might be better always generate params,
                    // and it is should be easy to do without JsonSerializer.Serialize call. On another side, there
                    // is no reason to write property in cases when it not required.
                    // TODO: whenever null is passed, don't serialize it? check chromium code, it
                    // generally also should accept null, so probably there is no sense to treat
                    // null case differently.
                    // if (!EqualityComparer<TRequest>.Default.Equals(parameters, default(TRequest)))
                    if (typeof(TRequest) != typeof(Unit))
                    {
                        encoder.WritePropertyName(StjEncodedProperties.Params);
                        // var typeInfo = StjTypeInfoResolver.GetTypeInfo<TRequest>();
                        JsonSerializer.Serialize<TRequest>(encoder, parameters,
                            StjTypeInfoResolver.JsonSerializerOptions);
                    }

                    if (_sessionId.Length != 0)
                    {
                        encoder.WriteString(StjEncodedProperties.SessionId, _jsonEncodedSessionId);
                    }
                    encoder.WriteEndObject();

                    encoder.Flush();

                    // TODO: Framing: RawWithTrailingZero
                    //if (_client.Framing == RawWithTrailingZero)
                    //{
                    //    var span = bufferWriter.GetSpan(1);
                    //    span[0] = 0;
                    //    bufferWriter.Advance(1);
                    //}

                    shouldReturnBufferWriter = false;
                    return bufferWriter;
                }
                finally
                {
                    Pools.Utf8JsonWriterPool.Return(encoder);
                }
            }
            finally
            {
                if (shouldReturnBufferWriter)
                {
                    Pools.BufferWriterPool.Return(bufferWriter);
                }
            }
        }

        #endregion

        #region Events

        private void AddEventDispatcher(string name, CrdtpDispatcher dispatcher) // TODO: make it public
        {
            // TODO: Support multiple dispatchers for same event.
            // Naive multi-dispatch implementation may have issue what same response will be deserialized multiple times.
            // However, to solve this issue there is possible to implement multi-cast multi-type delegate,
            // which will deserialize message for each type once, and invoke event handlers,
            // keeping objects inside. One important note, that in order, if there is multiple listeners,
            // but each for own type - then there is no need, to keep deserialized message to next event call.
            // Ideally add benchmark(s) to test performance considerations, because there is also
            // should not affect on common case with single dispatcher.
            //
            // Also there is good idea to think about "static"-like DomainDispatcher which will
            // dispatch/invoke methods directly. (It has own downsides, like possible deserialization
            // of event which we doesn't actually handle in concrete dispatcher, but there is probably
            // might be checked with reflection (e.g. determine if default/virtual/abstract handler not overriden).
            // However, there is concern that there is too complex without great benefits. However,
            // there is have sense to have ability to have DomainDispatchers (this would replaces event
            // subscription, and it is more controllable/flexible). But there is also have sense delay with this
            // once crdtp serialization would be ready.
            lock (EventDispatcherMapLock)
            {
                _eventDispatchers.Add(name, dispatcher);
            }
        }

        private void RemoveEventDispatcher(string name, CrdtpDispatcher dispatcher)
            => throw Error.NotImplemented();

        public void AddEventHandler<TEvent>(string name,
            EventHandler<TEvent> handler,
            object? sender = default,
            TaskRunner? taskRunner = default)
        {
            taskRunner ??= _taskRunner ?? _client._taskRunner;

            var dispatcher = new EventHandlerDispatcher<TEvent>(this, handler, sender, taskRunner);
            AddEventDispatcher(name, dispatcher);
        }

        public void AddEventHandler(string name,
            EventHandler handler,
            object? sender = default,
            TaskRunner? taskRunner = default)
            => throw Error.NotImplemented();

        public bool RemoveEventHandler<TEvent>(string name,
            EventHandler<TEvent> handler,
            object? sender = default)
            => throw Error.NotImplemented();

        public bool RemoveEventHandler(string name,
            EventHandler handler,
            object? sender = default)
            => throw Error.NotImplemented();

        #endregion

        public int GetNumberOfPendingRequests() => _numberOfPendingRequests;

        internal void DispatchEventInternal(Dispatchable dispatchable)
        {
            DebugCheck.That(dispatchable.SessionId == _sessionId);
            DebugCheck.That(!dispatchable.CallId.HasValue);
            DebugCheck.That(!string.IsNullOrEmpty(dispatchable.Method));

            CrdtpDispatcher? dispatcher = null;
            lock (_eventDispatchers)
            {
                _eventDispatchers.TryGetValue(dispatchable.Method!, out dispatcher);
            }
            if (dispatcher == null)
                return;

            var context = new CrdtpDispatchContext(this);
            dispatcher?.Dispatch(context, dispatchable);
        }

        private void ThrowIfNotAttached() // TODO: Inline
        {
            if (!_isAttached) ThrowNotAttached();
        }

        [DoesNotReturn]
        private static void ThrowNotAttached() // TODO: NoInline
        {
            // TODO: Simplify message.
            throw Error.InvalidOperation($"{nameof(CrdtpSession)} in invalid state (\"Detached\") for this operation. Valid states are: \"Attached\".");
        }

        private sealed class DefaultSessionHandler : CrdtpSessionHandler
        {
            public static DefaultSessionHandler Instance { get; } = new();

            private DefaultSessionHandler() { }

            protected internal override void OnAttach() { }

            protected internal override void OnDetach() { }

            protected internal override CrdtpErrorResponse? OnBeforeSend(JsonEncodedText method)
                => null;
        }
    }
}
