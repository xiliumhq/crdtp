using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Xilium.Crdtp.Client.Tests
{
    public class CrdtpClientTests
    {
        internal sealed class TestConnection : CrdtpConnection
        {
            public TestConnection(CrdtpConnectionDelegate @delegate)
                : base(@delegate)
            { }

            protected override void DisposeCore() { }

            protected override CrdtpConnectionReader? CreateReader() => null;

            protected override Task OpenAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override Task CloseAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
                => default;
        }

        [Fact]
        public void Create()
        {
            var client = new CrdtpClient((handler) => new TestConnection(handler));
            Assert.Equal(CrdtpClientState.None, client.State);
        }

        [Fact]
        public void Attach()
        {
            var client = new CrdtpClient((handler) => new TestConnection(handler));
            Assert.Equal(CrdtpClientState.None, client.State);

            var session = new CrdtpSession(sessionId: null, handler: null);
            client.Attach(session);
        }
    }
}
