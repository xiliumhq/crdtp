namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): Also create non-generic CrdtpResponse to use it in bindings in Try methods,
    // which might be used instead CrdtpResponse<Unit>. It should not be too hard to do, and be better
    // at user code.

    public readonly struct CrdtpResponse<T>
    {
        private readonly CrdtpErrorResponse? _error;
        private readonly T _result;

        public CrdtpResponse(CrdtpErrorResponse error)
        {
            Check.That(error != null);
            _error = error;
            _result = default!;
        }

        public CrdtpResponse(T result)
        {
            _error = null;
            _result = result;
        }

        public T GetResult()
        {
            if (_error != null) throw new CrdtpErrorResponseException(_error);
            return _result;
        }
    }
}
