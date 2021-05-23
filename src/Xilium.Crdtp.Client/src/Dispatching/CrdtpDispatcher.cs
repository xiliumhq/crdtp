namespace Xilium.Crdtp.Client.Dispatching
{
    // TODO(dmitry.azaraev): CrdtpDispatcher might be public + public Dispatchable
    // for support pre-generated typed dispatchers. Also CrdtpRequest looks very similar to dispatcher,
    // just handle method results, not events, so probably them might be unified.
    internal abstract class CrdtpDispatcher
    {
        public abstract void Dispatch(CrdtpDispatchContext context, Dispatchable dispatchable);
    }
}
