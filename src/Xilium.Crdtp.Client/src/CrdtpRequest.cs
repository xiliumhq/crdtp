using Xilium.Crdtp.Client.Dispatching;

namespace Xilium.Crdtp.Client
{
    // CrdtpRequest State
    //   None
    //   Registered
    //   Sending / IOPending
    //   Sent (so waiting for response)
    //   Received
    //
    // Request might be canceled in two ways:
    //   By CrdtpSession::CancelRequest(...) (CancelPendingRequests)
    //   By provided CancellationToken
    //
    //   There is two cases of cancel pending requests:
    //     For implementation of CancelPendingRequests, we doesn't need to unregister requests from pending requests map.
    //     For any other cases we should remove request from request map (by callId).

    // CrdtpSession State
    //   None
    //   Attached
    //   Detached

    // CrdtpClient State => should expose CrdtpConnectionState?
    //   None
    //   Opened
    //   Closed
    //   Aborted

    // TODO(dmitry.azaraev): (Low) CrdtpRequest seems similar to CrdtpDispatcher, but no more identical.
    internal abstract class CrdtpRequest
    {
        public abstract void Cancel(bool removeFromRequestMap = true);

        public abstract void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable);
    }
}
