using System;
using System.Text.Json;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client.Dispatching
{
    // TODO(dmitry.azaraev): EventHandlerDispatcher might share common deserialization logic.
    internal sealed class EventHandlerDispatcher<TEvent> : CrdtpDispatcher
    {
        private readonly CrdtpSession _session;
        private readonly EventHandler<TEvent> _handler;
        private readonly object? _sender;

        public EventHandlerDispatcher(CrdtpSession session, EventHandler<TEvent> handler, object? sender)
        {
            _session = session;
            _handler = handler;
            _sender = sender;
        }

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
                var result = JsonSerializer.Deserialize<TEvent>(dispatchable.Data, context.Session.GetJsonSerializerOptions());
                if (typeof(TEvent) != typeof(Unit))
                {
                    Check.That(result != null); // TODO: Better error reporting. Not sure if protocol allow null in that case.
                }
                _handler(_sender ?? _session, result!); // TODO(dmitry.azaraev): (High) catch and report exceptions back to session.
            }
            else throw Error.InvalidOperation("Invalid data type."); // TODO: there is protocol violation, but we should always create correct Dispatchable, and handle this violation at higher level. Event message must not hold error.
        }
    }
}
