using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xilium.Crdtp.Client.Tests
{
    internal sealed class FakeConnectionDelegate : CrdtpConnectionDelegate
    {
        private readonly ITestOutputHelper _testOutput;

        private int _onOpenTimes;
        private int _onCloseTimes;
        private int _onAbortTimes;

        public FakeConnectionDelegate(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public bool OnOpenCalledOnce => _onOpenTimes == 1;
        public bool OnCloseCalledOnce => _onCloseTimes == 1;
        public bool OnAbortCalledOnce => _onAbortTimes == 1;

        public override void OnOpen()
        {
            Interlocked.Increment(ref _onOpenTimes);
            _testOutput.WriteLine($"{nameof(FakeConnectionDelegate)}::{nameof(OnOpen)}");
        }

        public override void OnClose()
        {
            Interlocked.Increment(ref _onCloseTimes);
            _testOutput.WriteLine($"{nameof(FakeConnectionDelegate)}::{nameof(OnClose)}");
        }

        public override void OnAbort(Exception? ex)
        {
            Interlocked.Increment(ref _onAbortTimes);
            _testOutput.WriteLine($"{nameof(FakeConnectionDelegate)}::{nameof(OnAbort)}: {ex?.Message}");
        }

        public override void OnMessage(ReadOnlySpan<byte> message)
        {
            _testOutput.WriteLine($"{nameof(FakeConnectionDelegate)}::{nameof(OnMessage)}: {message.Length} bytes received");
        }
    }
}
