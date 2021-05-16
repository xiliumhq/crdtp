namespace Xilium.Crdtp.Client.Dispatching
{
    public readonly struct CrdtpDispatchContext
    {
        private readonly CrdtpSession _session;

        public CrdtpDispatchContext(CrdtpSession session)
        {
            _session = session;
        }

        // TODO(dmitry.azaraev): AggresiveInline?
        public readonly CrdtpSession Session => _session;
    }
}
