using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Xilium.Crdtp.Client.Tests
{
#pragma warning disable IDE1006 // Naming Styles
    public class __RefactorMe_CrdtpSessionTests
#pragma warning restore IDE1006 // Naming Styles
    {
        internal sealed class FakeConnection : CrdtpConnection
        {
            private TaskCompletionSource? _observeNextSendTcs;

            public FakeConnection(CrdtpConnectionDelegate @delegate)
                : base(@delegate)
            { }

            /// <summary>
            /// Returns Task which will be completed on next SendAsync call.
            /// </summary>
            public Task ObserveNextSend()
            {
                _observeNextSendTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                return _observeNextSendTcs.Task;
            }

            protected override void DisposeCore() { }

            protected override Task OpenAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override Task CloseAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override CrdtpConnectionReader? CreateReader() => null;

            protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
            {
                _observeNextSendTcs?.SetResult();
                return ValueTask.CompletedTask;
            }
        }

        [Fact]
        public async Task CallMethodWithCancelledToken()
        {
            // TODO: Normally this code also should observe what SendAsync is not get called, nor message is serialized.

            var client = new CrdtpClient((handler) => new FakeConnection(handler));
            var session = new CrdtpSession(client, "");
            client.Attach(session);
            var sessionApi = new TestSessionApi(session);
            await client.OpenAsync();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var actualException = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sessionApi.DoCallAsync(cts.Token));

            // TODO: Verify correct tokens provided, probably in separate test
            // Assert.True(actualException.CancellationToken == cts.Token);

            await client.CloseAsync();
        }

        [Fact]
        public async Task CallMethodAndCancelRequest()
        {
            FakeConnection? connection = null;
            var client = new CrdtpClient((handler) => (connection = new FakeConnection(handler)));
            Assert.NotNull(connection);

            var session = new CrdtpSession(client, "");
            client.Attach(session);
            var sessionApi = new TestSessionApi(session);
            await client.OpenAsync();

            using var cts = new CancellationTokenSource();

            var doCallSentTask = connection.ObserveNextSend();
            var doCallTask = sessionApi.DoCallAsync(cts.Token);
            await doCallSentTask;
            cts.Cancel();

            var actualException = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => doCallTask);

            // Ensure what there is no pending requests remains.
            Assert.Equal(0, sessionApi.GetCrdtpSession().GetNumberOfPendingRequests());

            await client.CloseAsync();
        }
    }
}
