using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Xilium.Crdtp.Client.Tests
{
    public class ConnectionTests
    {
        private readonly ITestOutputHelper _testOutput;

        public ConnectionTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void Ctor_State()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);

            Assert.Equal(CrdtpConnectionState.None, testConnection.State);
        }

        [Fact]
        public void Dispose_State()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);

            testConnection.Dispose();
            Assert.Equal(CrdtpConnectionState.Closed, testConnection.State);
            Assert.True(testConnectionDelegate.OnCloseCalledOnce);

            // Dispose twice should not call delegate second time.
            testConnection.Dispose();
            Assert.Equal(CrdtpConnectionState.Closed, testConnection.State);
            Assert.True(testConnectionDelegate.OnCloseCalledOnce);
        }

        [Fact]
        public async Task OpenAsync_Success()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);

            await testConnection.OpenAsync();
            Assert.Equal(CrdtpConnectionState.Open, testConnection.State);
            Assert.True(testConnectionDelegate.OnOpenCalledOnce);
        }

        [Fact]
        public async Task OpenAsync_Failure()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput)
            {
                ThrowOnOpenAsync = true,
            };

            await Assert.ThrowsAsync<CrdtpConnectionException>(() => testConnection.OpenAsync());
            Assert.Equal(CrdtpConnectionState.Aborted, testConnection.State);
            Assert.True(testConnectionDelegate.OnAbortCalledOnce);
        }

        [Fact]
        public async Task CloseAsync_State()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);

            // Allow close only in Open state.
            await Assert.ThrowsAsync<InvalidOperationException>(() => testConnection.CloseAsync());
        }

        [Fact]
        public async Task CloseAsync_Success()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);
            await testConnection.OpenAsync();

            await testConnection.CloseAsync();
            Assert.Equal(CrdtpConnectionState.Closed, testConnection.State);
            // TODO: Fix me, it starts to fail by 9262c494e3c9869071a740b98e71718d1eecd826
            // commit. For connections which implements Reader is racy by design, because
            // when reader ends - it closes connection, and when we ask connection to close
            // it also ends.
            Assert.True(testConnectionDelegate.OnCloseCalledOnce);
        }

        [Fact]
        public async Task CloseAsync_Failure()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput)
            {
                ThrowOnCloseAsync = true,
            };
            await testConnection.OpenAsync();

            await Assert.ThrowsAsync<CrdtpConnectionException>(() => testConnection.CloseAsync());
            Assert.Equal(CrdtpConnectionState.Aborted, testConnection.State);
            Assert.True(testConnectionDelegate.OnAbortCalledOnce);
        }

        [Fact]
        public async Task Abort()
        {
            var testConnectionDelegate = new FakeConnectionDelegate(_testOutput);
            using var testConnection = new FakeConnection(testConnectionDelegate, _testOutput);
            await testConnection.OpenAsync();

            testConnection.Abort();
            Assert.Equal(CrdtpConnectionState.Aborted, testConnection.State);
            Assert.True(testConnectionDelegate.OnAbortCalledOnce);

            testConnection.Abort();
            Assert.Equal(CrdtpConnectionState.Aborted, testConnection.State);
            Assert.True(testConnectionDelegate.OnAbortCalledOnce);
        }
    }
}
