using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Buffers;
using Xilium.Crdtp.Client.Dispatching;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (CrdtpSession) Consider to use more simplier request queue, HashSet/Dictionary+lock is generally better,
    // or lock-free table, or some different. There is almost zero concurrency for single session.

    public sealed class CrdtpSession
    {
        private readonly CrdtpClient _client;
        private readonly string? _sessionId;
        private int _callIdGen;

        private readonly Dictionary<int, CrdtpRequest> _requests;
        private readonly Dictionary<string, CrdtpDispatcher> _eventDispatchers;

        private readonly JsonEncodedText _jsonEncodedSessionId;

        internal CrdtpSession(CrdtpClient client, string? sessionId)
        {
            _client = client;
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
            _requests = new Dictionary<int, CrdtpRequest>();
            _eventDispatchers = new Dictionary<string, CrdtpDispatcher>();
        }

        public string? SessionId => _sessionId;

        public int GetNextCallId() => Interlocked.Increment(ref _callIdGen);

        // TODO(dmitry.azaraev): (Low) AggresiveInline
        internal JsonSerializerOptions GetJsonSerializerOptions()
            => CrdtpClient.JsonSerializerOptionsBuilder.GetOptions();

        public void UseSerializerOptions(StjSerializerOptions options)
        {
            CrdtpClient.JsonSerializerOptionsBuilder.Add(options);
        }

        // TODO(dmitry.azaraev): CrdtpSession::Attaching/Detaching / GetRootSession(), AttachChild/DetachChild/HasChild?

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
            Debugger.NotifyOfCrossThreadDependency();
            Check.That(_client.Encoding == CrdtpEncoding.Json);

            var callId = GetNextCallId();

            var bufferWriter = SerializeRequest(callId, method, parameters);
            var shouldReturnBufferWriter = true;
            try
            {
                // now we can send buffer and release it, but before we should register request
                var request = new CrdtpRequest<TResponse>(callId);
                AddRequest(callId, request);
                // TODO(dmitry.azaraev): (High) Handle cancellation, e.g. once request is created and registered,
                // if send is fail or send, request should get somehow know about this situation.
                var sendTask = _client.SendAsync(bufferWriter.WrittenMemory, cancellationToken);
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
            finally
            {
                Pools.BufferWriterPool.Return(bufferWriter);
            }

            await request.Task.ConfigureAwait(false);
            return request.Task.Result;
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

        public void RemoveEventHandler<TEvent>(string name, EventHandler<TEvent> handler, object? sender = default)
            => throw Error.NotImplemented();

        public void RemoveEventHandler(string name, EventHandler handler, object? sender = default)
            => throw Error.NotImplemented();

        #endregion

        internal void OnAttached()
        {
        }

        internal void OnDetached()
        {
        }

        private void AddRequest(int callId, CrdtpRequest request)
        {
            lock (_requests)
            {
                _requests.Add(callId, request);
            }
        }

        private CrdtpRequest GetAndRemoveRequest(int callId)
        {
            lock (_requests)
            {
                if (!_requests.TryGetValue(callId, out var request))
                    throw Error.InvalidOperation("Request with given id not found.");

                _requests.Remove(callId);
                return request;
            }
        }

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

                if (dispatcher != null)
                {
                    dispatcher.Dispatch(context, dispatchable);
                }
                else
                {
                    // TODO(dmitry.azaraev): (Low) There is probably not bad idea to report events which we doesn't handle. Need API.
                    Console.WriteLine("EVT: No dispatcher for event: {0}", dispatchable.Method);
                }
            }
            else
            {
                // TODO(dmitry.azaraev): CrdtpSession: Report protocol violation.
                throw Error.InvalidOperation($"Protocol violation. Given message is not method result, nor event.");
            }
        }
    }
}
