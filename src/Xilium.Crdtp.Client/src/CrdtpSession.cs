using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Buffers;
using Xilium.Crdtp.Client.Dispatching;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (CrdtpSession::RequestMap) Consider to use specialized request map
    // (better do it as value type?), which will be optimized to common case
    // (e.g. where only few concurrent requests are exist).
    // TODO(dmitry.azaraev): (CrdtpSession) Consider to use more simplier request queue, HashSet/Dictionary+lock is generally better,
    // or lock-free table, or some different. There is almost zero concurrency for single session.

    public sealed class CrdtpSession : Api.ISessionApi
    {
        private readonly string? _sessionId;
        private readonly CrdtpSessionHandler? _handler;
        private int _callIdGen;

        internal readonly object StateAndRequestMapLock = new object();
        private CrdtpClient? _client;
        private bool _isAttached;
        private Dictionary<int, CrdtpRequest> _requests;

        internal readonly object EventDispatcherMapLock = new object();
        private readonly Dictionary<string, CrdtpDispatcher> _eventDispatchers;

        private readonly JsonEncodedText _jsonEncodedSessionId;

        public CrdtpSession(string? sessionId, CrdtpSessionHandler? handler = null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                _sessionId = null;
                _jsonEncodedSessionId = default;
            }
            else
            {
                _sessionId = sessionId;
                _jsonEncodedSessionId = JsonEncodedText.Encode(sessionId);
            }
            _handler = handler;
            _requests = new Dictionary<int, CrdtpRequest>();
            _eventDispatchers = new Dictionary<string, CrdtpDispatcher>();
        }

        public string? SessionId => _sessionId;

        public bool IsAttached => _isAttached;

        public CrdtpClient GetClient()
        {
            var client = _client;
            if (client == null) throw Error.InvalidOperation("Session is not attached.");
            return client;
        }

        public CrdtpClient? GetClientOrDefault() => _client;

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
                _client = client;
                _handler?.OnAttach();
            }
        }

        internal void DetachInternal()
        {
            DebugCheck.That(Monitor.IsEntered(StateAndRequestMapLock), $"{nameof(StateAndRequestMapLock)} should be held.");

            if (_isAttached)
            {
                _isAttached = false;
                _client = null;
                CancelPendingRequests(); // TODO: this can be called outside of the lock, or from Attach/Detach methods
                _handler?.OnDetach();
            }
        }

        public void CancelPendingRequests() // TODO: Review use of CancelPendingRequests. When it is needed - use Internal version.
        {
            lock (StateAndRequestMapLock)
            {
                if (_requests.Count == 0) return;

                var requests = _requests;

                // TODO: This dictionary should not be accessed during cancellation.
                // And we doesn't want take lock on registered cancellation token callback.
                // This might requires to add additional internal state, which will block
                // adding new requests, then we might swap dictionaries, and release lock.
                // After this cancel pending requests (concurrently).
                // And after all pending requests are cancelled - take lock and set correct state back (Open_Cancelling -> Open)
                // This requres to make threaded test, might be done with fake connection.
                _requests = null!;

                foreach (var request in requests.Values)
                {
                    // TODO: Does it safe to execute cancel? (request can be cancelled concurrently via cancellation token => and it should attempt to unreginster request from this collection and take lock)?
                    request.Cancel(removeFromRequestMap: false);
                }

                requests.Clear();
                _requests = requests;
            }
        }

        #endregion

        // TODO(dmitry.azaraev): (Low) AggresiveInline
        public int GetNextCallId() => Interlocked.Increment(ref _callIdGen);

        // TODO(dmitry.azaraev): (Low) AggresiveInline
        internal JsonSerializerOptions GetJsonSerializerOptions()
            => CrdtpClient.JsonSerializerOptionsBuilder.GetOptions();

        public void UseSerializerOptions(StjSerializerOptions options)
            => CrdtpClient.JsonSerializerOptionsBuilder.Add(options);


        #region Commands

        public async Task ExecuteCommandAsync<TRequest>(
            string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(
                method, parameters, cancellationToken).ConfigureAwait(false);
            _ = response.GetResult();
        }

        public async Task ExecuteCommandAsync<TRequest>(
            JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, Unit>(
                method, parameters, cancellationToken).ConfigureAwait(false);
            _ = response.GetResult();
        }

        public async Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
            string method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, TResponse>(method,
                parameters, cancellationToken).ConfigureAwait(false);
            return response.GetResult();
        }

        public async Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
            JsonEncodedText method,
            TRequest parameters,
            CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync<TRequest, TResponse>(method,
                parameters, cancellationToken).ConfigureAwait(false);
            return response.GetResult();
        }

        // TODO(dmitry.azaraev): (Low) Add SendCommandAsync<TRequest> which might return CrdtpResponse?

        // TODO: Generic sending method for sending command(s).
        // This method is not necessary might be accessible, if client is built without Json/Dynamic support.
        // [DynamicallyAccessedMembers(MembersAccessedOnWrite)] => public fields & public properties
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
            // cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotAttached();
            // Debugger.NotifyOfCrossThreadDependency();
            // TODO: Enable checks above when tests be more robust.

            var client = _client;
            if (client == null) ThrowNotAttached();

            Check.That(client.Encoding == CrdtpEncoding.Json);

            var callId = GetNextCallId();
            var bufferWriter = SerializeRequest(callId, method, parameters);
            var shouldReturnBufferWriter = true;
            try
            {
                // now we can send buffer and release it, but before we should register request
                var request = new CrdtpRequest<TResponse>(this, callId); // cancellationToken);

                lock (StateAndRequestMapLock)
                {
                    ThrowIfNotAttached();

                    if (!_requests.TryAdd(callId, request))
                    {
                        throw Error.InvalidOperation("Request with same id already registered.");
                    }
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
                    if (typeof(TRequest) != typeof(Unit))
                    {
                        encoder.WritePropertyName(StjEncodedProperties.Params);
                        JsonSerializer.Serialize(encoder, parameters, GetJsonSerializerOptions());
                    }

                    if (!string.IsNullOrEmpty(_sessionId))
                    {
                        encoder.WriteString(StjEncodedProperties.SessionId, _jsonEncodedSessionId);
                    }
                    encoder.WriteEndObject();

                    encoder.Flush();

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

        internal void AddEventDispatcher(string name, CrdtpDispatcher dispatcher) // TODO: make it public
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
            lock (_eventDispatchers)
            {
                _eventDispatchers.Add(name, dispatcher);
            }
        }

        internal void RemoveEventDispatcher(string name, CrdtpDispatcher dispatcher)
            => throw Error.NotImplemented();

        public void AddEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default)
        {
            var dispatcher = new EventHandlerDispatcher<TEvent>(this, handler, sender);
            AddEventDispatcher(name, dispatcher);
        }

        public void AddEventHandler(string name, EventHandler handler, object? sender = default)
            => throw Error.NotImplemented();

        public bool RemoveEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default)
            => throw Error.NotImplemented();

        public bool RemoveEventHandler(string name, EventHandler handler, object? sender = default)
            => throw Error.NotImplemented();

        #endregion

        private CrdtpRequest GetAndRemoveRequest(int callId)
        {
            lock (StateAndRequestMapLock)
            {
                if (!_requests.TryGetValue(callId, out var request))
                    throw Error.InvalidOperation("Request with given id not found."); // TODO: Emit event

                _requests.Remove(callId);
                return request;
            }
        }

        internal bool UnregisterRequest(int callId, CrdtpRequest request)
        {
            lock (StateAndRequestMapLock)
            {
                // TODO: This would be nice to have TryRemove method, rather than this two calls.
                // Even if we remove unrelated request, we may always add it back.
                if (_requests.TryGetValue(callId, out var actualRequest))
                {
                    if (actualRequest == (object)request)
                    {
                        return _requests.Remove(callId);
                    }
                }
                return false;
            }
        }

        [Obsolete("This method will be removed soon.")]
        public int GetNumberOfPendingRequestsForTest() => _requests.Count;

        internal void Dispatch(Dispatchable dispatchable)
        {
            DebugCheck.That(dispatchable.SessionId == _sessionId);

            var context = new CrdtpDispatchContext(this);

            if (dispatchable.CallId.HasValue)
            {
                // TODO(dmitry.azaraev): (High) CrdtpSession: Handle if request not found.
                var request = GetAndRemoveRequest(dispatchable.CallId.Value);
                request.Dispatch(context, dispatchable);
            }
            else if (!string.IsNullOrEmpty(dispatchable.Method))
            {
                CrdtpDispatcher? dispatcher = null;
                lock (_eventDispatchers)
                {
                    _eventDispatchers.TryGetValue(dispatchable.Method, out dispatcher);
                }
                dispatcher?.Dispatch(context, dispatchable);
            }
            else
            {
                // TODO(dmitry.azaraev): CrdtpSession: Report protocol violation.
                throw Error.InvalidOperation($"Protocol violation. Given message is not method result nor event notification.");
            }
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
    }
}
