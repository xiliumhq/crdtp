using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client.Tests
{
    internal readonly struct TestSessionApi
    {
        private readonly CrdtpSession _session;

        public TestSessionApi(CrdtpSession session)
        {
            _session = session;
        }

        public CrdtpSession GetCrdtpSession() => _session;

        public Task<EmptyResponse> DoCallAsync(CancellationToken cancellationToken)
        {
            // TODO: Add check what we should do if null provided for request.
            return _session.ExecuteCommandAsync<EmptyRequest, EmptyResponse>(
                "Test.doCall", new(), cancellationToken);
        }

        public sealed class EmptyRequest { }
        public sealed class EmptyResponse { }
    }
}
