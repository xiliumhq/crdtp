namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): Also create non-generic CrdtpResponse to use it in bindings in Try methods,
    // which would be used instead CrdtpResponse<Unit>.

    public readonly struct CrdtpResponse<TResult>
    {
        private readonly CrdtpError? _error;
        private readonly TResult _result;

        public CrdtpResponse(CrdtpError error)
        {
            Check.That(error != null);
            _error = error;
            _result = default!;
        }

        public CrdtpResponse(TResult result)
        {
            _error = null;
            _result = result;
        }

        public TResult GetResult()
        {
            if (_error != null) throw new CrdtpErrorException(_error);
            return _result;
        }
    }
}
