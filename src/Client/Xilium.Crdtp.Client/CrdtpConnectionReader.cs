using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (?) CrdtpConnectionReader: Needs option to run reader on separate thread.

    public abstract class CrdtpConnectionReader
    {
        private readonly CancellationTokenSource _cts;
        private Task? _task;

        protected CrdtpConnectionReader()
        {
            _cts = new CancellationTokenSource();
        }

        internal Task Task => _task;

        internal void RunAsync()
        {
            // TODO(dmitry.azaraev): CrdtpConnectionReader: when read loop finished, ensure / set connection state.
            _task = RunReadLoopAsync(_cts.Token);
        }

        internal async Task StopAsync()
        {
            _cts.Cancel();

            if (_task != null)
            {
                await _task.ConfigureAwait(false);
            }
        }

        protected abstract Task RunReadLoopAsync(CancellationToken cancellationToken);
    }
}
