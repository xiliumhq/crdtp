using Xilium.Crdtp.Client.Dispatching;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Low) CrdtpRequest seems identical to CrdtpDispatcher.
    internal abstract class CrdtpRequest
    {
        public abstract void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable);
    }
}
