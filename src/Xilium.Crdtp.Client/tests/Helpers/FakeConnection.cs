using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xilium.Crdtp.Client.Tests
{
    internal sealed class FakeConnection : CrdtpConnection
    {
        private readonly ITestOutputHelper _testOutput;

        public FakeConnection(CrdtpConnectionDelegate @delegate, ITestOutputHelper testOutput)
            : base(@delegate)
        {
            _testOutput = testOutput;
        }

        public bool ThrowOnOpenAsync { get; set; }
        public bool ThrowOnCloseAsync { get; set; }

        protected override void DisposeCore() { }

        protected override Task OpenAsyncCore(CancellationToken cancellationToken)
        {
            if (ThrowOnOpenAsync) throw new InvalidOperationException($"{nameof(FakeConnection)}::{nameof(OpenAsyncCore)}: {nameof(ThrowOnOpenAsync)} = true.");
            return Task.CompletedTask;
        }

        protected override Task CloseAsyncCore(CancellationToken cancellationToken)
        {
            if (ThrowOnCloseAsync) throw new InvalidOperationException($"{nameof(FakeConnection)}::{nameof(CloseAsyncCore)}: {nameof(ThrowOnCloseAsync)} = true.");
            return Task.CompletedTask;
        }

        protected override CrdtpConnectionReader? CreateReader() => null;

        protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
            => ValueTask.CompletedTask;
    }
}
