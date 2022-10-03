using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Xilium.Crdtp.Client.Tests;

/// <summary>
/// Test reproduces deadlock when `CrdtpConnection::Dispose` and `CrdtpClient::Dispose`
/// called concurrently.
/// </summary>
public class CrdtpClientDisposeDeadlockTest
{
    internal sealed class TestConnection : CrdtpConnection
    {
        private readonly TaskCompletionSource _readDoneTcs;

        public TestConnection(CrdtpConnectionDelegate @delegate)
            : base(@delegate)
        {
            _readDoneTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        protected override void DisposeCore()
        {
            _ = _readDoneTcs.TrySetResult();
        }

        protected override CrdtpConnectionReader? CreateReader()
            => new TestConnectionReader(_readDoneTcs);

        protected override Task OpenAsyncCore(CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override Task CloseAsyncCore(CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
            => default;

        private class TestConnectionReader : CrdtpConnectionReader
        {
            private readonly TaskCompletionSource _readDoneTcs;

            public TestConnectionReader(TaskCompletionSource readDoneTcs)
            {
                _readDoneTcs = readDoneTcs;
            }

            protected override async Task RunReadLoopAsync(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // await Task.Yield();
                    await _readDoneTcs.Task.ConfigureAwait(false);
                }
                ;
            }
        }
    }

    [Fact]
    public async Task RunAsync()
    {
        for (var i = 0; i < 1000; i++)
        {
            TestConnection testConnection = null!;
            var client = new CrdtpClient(
                (handler) => testConnection = new TestConnection(handler));
            Assert.Equal(CrdtpClientState.None, client.State);

            await client.OpenAsync();
            Assert.Equal(CrdtpClientState.Open, client.State);

            var testConnectionDisposeTask = Task.Factory.StartNew(() =>
            {
                testConnection.Dispose();
            }, TaskCreationOptions.LongRunning);
            client.Dispose();
            await testConnectionDisposeTask;

            Assert.Equal(CrdtpClientState.Closed, client.State);
            // Aborted state also okay.
            // Assert.True(client.State == CrdtpClientState.Aborted);
        }
    }
}
