using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using Xilium.Crdtp.Core;
using Xilium.Threading;

namespace Xilium.Crdtp.Client.Dispatching
{
    // TODO(dmitry.azaraev): EventHandlerDispatcher might share common deserialization logic.
    internal sealed class EventHandlerDispatcher<
#if XI_CRDTP_TRIMMABLE_DYNAMIC
        [DynamicallyAccessedMembers(Compat.ForDeserialization)]
#endif
    TEvent> : CrdtpDispatcher
    {
        // private readonly CrdtpSession _session;
        private readonly EventHandler<TEvent> _handler;
        private readonly object? _sender;
        private readonly TaskRunner? _taskRunner;
        private readonly SendOrPostCallback _cachedAction;

        public EventHandlerDispatcher(CrdtpSession session,
            EventHandler<TEvent> handler,
            object? sender,
            TaskRunner? taskRunner)
        {
            // _session = session;
            _handler = handler;
            _sender = sender ?? session;
            _taskRunner = taskRunner;

            _cachedAction = (object? arg) =>
            {
                // TODO(dmitry.azaraev): (High) catch and report exceptions back to session.
                _handler(_sender, (TEvent)arg!);
            };
        }

#if XI_CRDTP_TRIMMABLE_DYNAMIC
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "TEvent type is preserved by the DynamicallyAccessedMembers.")]
#endif
        public override void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable)
        {
            DebugCheck.That(!dispatchable.CallId.HasValue && !string.IsNullOrEmpty(dispatchable.Method));

            // TODO(dmitry.azaraev): This code looks too similar with CrdtpRequestOfT.
            // TODO: When result is not passed over protocol - empty / fake empty result should be used.
            if (dispatchable.DataType == Dispatchable.PayloadType.Params)
            {
                // TODO: For Unit type we can just skip deserialization completely.
                // In this case JsonConverter<Unit> will not be needed at all.

                // params might be null?
                var result = JsonSerializer.Deserialize<TEvent>(dispatchable.Data, context.Session.StjTypeInfoResolver.JsonSerializerOptions);
                if (typeof(TEvent) != typeof(Unit))
                {
                    Check.That(result != null); // TODO: Better error reporting. Not sure if protocol allow null in that case.
                }

                if (_taskRunner != null)
                {
                    _taskRunner.PostTask(_cachedAction, result);
                }
                else
                {
                    _handler(_sender, result!); // TODO(dmitry.azaraev): (High) catch and report exceptions back to session.
                }
            }
            else throw Error.InvalidOperation("Invalid data type."); // TODO: there is protocol violation, but we should always create correct Dispatchable, and handle this violation at higher level. Event message must not hold error.
        }
    }
}
