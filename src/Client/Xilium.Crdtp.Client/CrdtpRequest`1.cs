using System.Text.Json;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Dispatching;
using Xilium.Crdtp.Core;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): CrdtpRequest<TResult>: should hold bare minimum for processing request.
    // There is no need to hold _callId in this class. Alternatively may use HashSet instead of Dictionary,
    // and key by _callId field.
    internal sealed class CrdtpRequest<TResult> : CrdtpRequest
    {
        private readonly int _callId;
        private readonly TaskCompletionSource<CrdtpResponse<TResult>> _tcs;

        public CrdtpRequest(int callId)
        {
            _callId = callId;
            _tcs = new TaskCompletionSource<CrdtpResponse<TResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task<CrdtpResponse<TResult>> Task => _tcs.Task;

        public override void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable)
        {
            DebugCheck.That(dispatchable.CallId.HasValue && dispatchable.CallId.Value == _callId);

            // TODO(dmitry.azaraev): This code looks too similar with EventHandlerDispatcher.
            // TODO: When result is not passed over protocol - empty / fake empty result should be used.
            if (dispatchable.DataType == Dispatchable.PayloadType.Result)
            {
                // TODO: For Unit type we can just skip deserialization completely.
                // In this case JsonConverter<Unit> will not be needed at all.

                // params might be null?
                var result = JsonSerializer.Deserialize<TResult>(dispatchable.Data, context.Session.GetJsonSerializerOptions());
                if (typeof(TResult) != typeof(Unit))
                {
                    Check.That(result != null); // TODO: Better error reporting, nulls are invalid or valid?
                }
                _tcs.SetResult(new CrdtpResponse<TResult>(result!));
            }
            else if (dispatchable.DataType == Dispatchable.PayloadType.Error)
            {
                var error = JsonSerializer.Deserialize<CrdtpError>(dispatchable.Data, context.Session.GetJsonSerializerOptions());
                Check.That(error != null);
                _tcs.SetResult(new CrdtpResponse<TResult>(error));
            }
            else throw Error.InvalidOperation("Invalid data type.");
        }
    }
}
