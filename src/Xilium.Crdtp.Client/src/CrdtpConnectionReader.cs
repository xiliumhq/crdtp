using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Low) CrdtpConnectionReader: Needs option to run reader on separate thread.

    public abstract class CrdtpConnectionReader : IDisposable
    {
        private readonly CancellationTokenSource _cts;
        private Task? _task;
        private bool _disposed;

        protected CrdtpConnectionReader()
        {
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    _cts.Cancel();
                    _cts.Dispose();
                }
            }
        }

        internal Task StartAsync()
        {
            DebugCheck.That(!_cts.IsCancellationRequested);
            DebugCheck.That(_task == null);

            return _task = RunReadLoopAsync(_cts.Token);
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
