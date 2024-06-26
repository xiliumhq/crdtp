﻿#if NET5_0_OR_GREATER
#define HAS_CANCELLATIONTOKEN_UNSAFEREGISTER
#define HAS_CANCELLATIONTOKENREGISTRATION_TOKEN
#else
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Dispatching;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client
{
    // TODO: HandleUnexpectedCancellation - (cancellationToken.CanBeCanceled && !cancellationToken.IsCancellationRequested)
    // in such cases it might be possible?

    // TODO: Eventually it should be CrdtpRequestCompletionSource<TResult> which
    // derived from TaskCompletionSource<TResponse>, ICrdtpRequest
    // This would give less memory allocations.
    internal sealed class CrdtpRequest<
#if XI_CRDTP_TRIMMABLE_DYNAMIC
        [DynamicallyAccessedMembers(Compat.ForDeserialization)]
#endif
    TResponse> : CrdtpRequest
    {
        // private static readonly Action<object?> s_cancelRequestAction = OnCancel;
        private readonly int _callId;
        private readonly TaskCompletionSource<CrdtpResponse<TResponse>> _tcs;
        private CancellationTokenRegistration _cancellationTokenRegistration;

#if DEBUG
        private bool _cancellationHasBeenRegistered;
#endif

        public CrdtpRequest(CrdtpSession session, int callId)
        {
            _callId = callId;

            var creationOptions = session.TaskRunner == null ?
                TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None;
            _tcs = new TaskCompletionSource<CrdtpResponse<TResponse>>(session, creationOptions);
        }

        public override CrdtpSession Session => (CrdtpSession)_tcs.Task.AsyncState!;

        private void ReleaseCancellationTokenRegistration()
        {
            _cancellationTokenRegistration.Dispose();
        }

        internal void RegisterForCancellation(CancellationToken cancellationToken)
        {
#if DEBUG
            DebugCheck.That(!_cancellationHasBeenRegistered, "Cannot register for cancellation twice");
            _cancellationHasBeenRegistered = true;
#endif
            // If request is already completed (for example in case of synchronous response)
            // then there is no ever needed to register cancellation token.
            if (_tcs.Task.IsCompleted)
            {
                return;
            }

            _cancellationTokenRegistration = cancellationToken
#if HAS_CANCELLATIONTOKEN_UNSAFEREGISTER
                .UnsafeRegister
#else
                .Register
#endif
                    (CrdtpRequestCompletionSourceHelper.HandleCancelDelegate, this);
        }

        public override void Cancel(bool removeFromRequestMap = true)
        {
            ReleaseCancellationTokenRegistration();
            CancelInternal(removeFromRequestMap);
        }

        private void CancelInternal(bool removeFromRequestMap = true)
        {
#if HAS_CANCELLATIONTOKENREGISTRATION_TOKEN
            if (_tcs.TrySetCanceled(_cancellationTokenRegistration.Token))
#else
            if (_tcs.TrySetCanceled())
#endif
            {
                if (removeFromRequestMap)
                {
                    var unregistered = Session._client.UnregisterRequest(_callId, this);
                    DebugCheck.That(unregistered);
                }
            }
        }

        public void Abort(Exception exception)
        {
            ReleaseCancellationTokenRegistration();
            if (_tcs.TrySetException(exception))
            {
                var unregistered = Session._client.UnregisterRequest(_callId, this);
                DebugCheck.That(unregistered);
            }
        }

#if XI_CRDTP_TRIMMABLE_DYNAMIC
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "TResponse type is preserved by the DynamicallyAccessedMembers.")]
#endif
        public override void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable)
        {
            // At this moment we are sure what response is dispatched, and we must set one
            // of outcome state.
            ReleaseCancellationTokenRegistration();
            try
            {
                DebugCheck.That(dispatchable.CallId.HasValue && dispatchable.CallId.Value == _callId);
                DebugCheck.That(context.Session == Session);

                if (_tcs.Task.IsCanceled) return;
                DebugCheck.That(!_tcs.Task.IsFaulted);

                // TODO: This method always should call Release() before deserialization occurs,
                // and always call one of TrySetXxx methods.

                // TODO(dmitry.azaraev): This code looks too similar with EventHandlerDispatcher.
                // TODO: When result is not passed over protocol - empty / fake empty result should be used.
                if (dispatchable.DataType == Dispatchable.PayloadType.Result)
                {
                    // TODO: For Unit type we can just skip deserialization completely.
                    // In this case JsonConverter<Unit> will not be needed at all.

                    // params might be null?
                    var result = JsonSerializer.Deserialize<TResponse>(dispatchable.Data, context.Session.StjTypeInfoResolver.JsonSerializerOptions);
                    if (typeof(TResponse) != typeof(Unit))
                    {
                        Check.That(result != null); // TODO: Better error reporting, nulls are invalid or valid?
                    }

                    var response = new CrdtpResponse<TResponse>(result!);
                    var taskRunner = Session.TaskRunner;
                    if (taskRunner != null)
                    {
                        // TODO: This should be optimized, to avoid closure allocation
                        taskRunner.PostTask(() => { _ = _tcs.TrySetResult(response); });
                    }
                    else
                    {
                        _ = _tcs.TrySetResult(response);
                    }
                }
                else if (dispatchable.DataType == Dispatchable.PayloadType.Error)
                {
                    var error = DeserializeError(dispatchable.Data, context.Session.StjTypeInfoResolver.JsonSerializerOptions);
                    Check.That(error != null);

                    var response = new CrdtpResponse<TResponse>(error);
                    var taskRunner = Session.TaskRunner;
                    if (taskRunner != null)
                    {
                        // TODO: This should be optimized, to avoid closure allocation
                        taskRunner.PostTask(() => { _ = _tcs.TrySetResult(response); });
                    }
                    else
                    {
                        _ = _tcs.TrySetResult(response);
                    }
                }
                else throw Error.InvalidOperation("Invalid data type.");
            }
            catch (Exception ex)
            {
                var taskRunner = Session.TaskRunner;
                if (taskRunner != null)
                {
                    // TODO: This should be optimized, to avoid closure allocation
                    taskRunner.PostTask(() => { _ = _tcs.TrySetException(ex); });
                }
                else
                {
                    _ = _tcs.TrySetException(ex);
                }
            }
        }

        public Task<CrdtpResponse<TResponse>> Task => _tcs.Task;

#if XI_CRDTP_TRIMMABLE_DYNAMIC
        [DynamicDependency(Compat.ForDeserialization, typeof(CrdtpErrorResponse))]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "The type is preserved by the DynamicDependency.")]
#endif
        private static CrdtpErrorResponse? DeserializeError(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<CrdtpErrorResponse>(utf8Json, options);
    }
}
