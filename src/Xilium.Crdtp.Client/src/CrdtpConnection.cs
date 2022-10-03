// #define XI_CRDTPCONNECTION_ASYNCDISPOSABLE

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client
{
    public abstract partial class CrdtpConnection : IDisposable
#if XI_CRDTPCONNECTION_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
    {
        protected readonly CrdtpConnectionDelegate Delegate;

        private readonly object StateUpdateLock = new object();
        private CrdtpConnectionState _state = CrdtpConnectionState.None;

        private bool _disposed;
        private CrdtpConnectionReader? _reader;

        protected CrdtpConnection(CrdtpConnectionDelegate @delegate)
        {
            Check.Argument.NotNull(@delegate, nameof(@delegate));
            Delegate = @delegate;
        }

        #region IDisposable

        /// <summary>
        /// Disposes the <see cref="CrdtpConnection"/>.
        /// </summary>
        // TODO: (Documentation) Document CrdtpConnection::Dispose behavior.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="CrdtpConnection"/>.
        /// </summary>
        /// <param name="disposing">If true, the <see cref="CrdtpConnection"/> is being disposed. If false, the <see cref="CrdtpConnection"/> is being finalized.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                bool shouldDispose = false;
                lock (StateUpdateLock)
                {
                    if (!_disposed)
                    {
                        _disposed = shouldDispose = true;
                        if (_state < CrdtpConnectionState.Closing)
                        {
                            _state = CrdtpConnectionState.Closing;
                        }
                    }
                }

                if (shouldDispose)
                {
                    List<Exception>? exceptions = null;
                    try
                    {
                        _reader?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        (exceptions ??= new()).Add(ex);
                    }

                    try
                    {
                        DisposeCore();
                    }
                    catch (Exception ex)
                    {
                        (exceptions ??= new()).Add(ex);
                    }

                    lock (StateUpdateLock)
                    {
                        if (_state < CrdtpConnectionState.Closed)
                        {
                            _state = exceptions == null ?
                                CrdtpConnectionState.Closed :
                                CrdtpConnectionState.Aborted;
                        }
                    }

                    if (exceptions == null)
                        Delegate.OnClose();
                    else
                        Delegate.OnAbort(exceptions);
                }
            }
        }

        protected abstract void DisposeCore();

        #endregion

        #region AsyncDisposable
#if XI_CRDTPCONNECTION_ASYNCDISPOSABLE

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the <see cref="CrdtpConnection"/>.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        protected virtual ValueTask DisposeAsyncCore()
        {
            Dispose(true);
            return default;
        }

#endif
        #endregion

        public CrdtpConnectionState State => _state;

        public virtual CrdtpEncoding Encoding => CrdtpEncoding.Json;

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            lock (StateUpdateLock)
            {
                ThrowIfInvalidState(CrdtpConnectionState.None);
                _state = CrdtpConnectionState.Connecting;
            }

            try
            {
                await OpenAsyncCore(cancellationToken).ConfigureAwait(false);
            }
            catch (CrdtpConnectionException ccex)
            {
                Abort(ccex);
                throw;
            }
            catch (Exception e)
            {
                var ccex = new CrdtpConnectionException("Failed to open connection.", e);
                Abort(e);
                throw ccex;
            }

            try
            {
                _reader = CreateReader();
            }
            catch (Exception e)
            {
                var ccex = new CrdtpConnectionException("Failed to create connection reader.", e);
                Abort(e);
                throw ccex;
            }

            lock (StateUpdateLock)
            {
                ThrowIfInvalidState(CrdtpConnectionState.Connecting);
                _state = CrdtpConnectionState.Open;
            }

            Delegate.OnOpen();

            StartReader();
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            lock (StateUpdateLock)
            {
                ThrowIfInvalidState(CrdtpConnectionState.Open);
                _state = CrdtpConnectionState.Closing;
            }

            // TODO: (High) CloseAsync & reader should be stopped regardless to possible
            // exception in CloseAsyncCore, but catch both exceptions and rethrow
            // only if necessary.
            try
            {
                await CloseAsyncCore(cancellationToken).ConfigureAwait(false);
                if (_reader != null)
                {
                    await _reader.StopAsync().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                var ccex = new CrdtpConnectionException("Failed to close connection.", e);
                Abort(e);
                throw ccex;
            }

            // TODO: (Low) CrdtpConnection: This explicit transition might be not necessary, because Dispose() already do that.
            lock (StateUpdateLock)
            {
                if (_state == CrdtpConnectionState.Closing)
                {
                    _state = CrdtpConnectionState.Closed;
                }
            }

            // TODO: OnAbort & OnClose. We should call abort or close, not both?
            Delegate.OnClose();
            Dispose();
        }

        public void Abort(Exception? exception = null)
        {
            lock (StateUpdateLock)
            {
                var state = _state;
                if (state == CrdtpConnectionState.Closed
                    || state == CrdtpConnectionState.Aborted)
                {
                    return;
                }

                _state = CrdtpConnectionState.Aborted;
            }

            // TODO: OnAbort & OnClose. We should call abort or close, not both?
            Delegate.OnAbort(exception);
            Dispose();
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> message)
            => SendAsyncCore(message);

        protected abstract Task OpenAsyncCore(CancellationToken cancellationToken);

        protected abstract Task CloseAsyncCore(CancellationToken cancellationToken);

        /// <remarks>
        /// Note, this method intentionally does not accept cancellation token, because there is no
        /// way to truly cancel sending without breaking communication pipe at the same time.
        /// </remarks>
        protected abstract ValueTask SendAsyncCore(ReadOnlyMemory<byte> message);

        protected abstract CrdtpConnectionReader? CreateReader();

        private void StartReader()
        {
            if (_reader == null) return;

            try
            {
                _ = _reader.StartAsync().ContinueWith((task) =>
                {
                    switch (task.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            Dispose();
                            break;

                        case TaskStatus.Faulted:
                            Abort(new CrdtpConnectionException("Reader faulted.",
                                task.Exception?.GetBaseException()));
                            break;

                        case TaskStatus.Canceled:
                            Abort(new CrdtpConnectionException("Unreachable: Reader cancelled.", task.Exception));
                            DebugCheck.Unreachable();
                            break;

                        default:
                            Abort(Error.Unreachable());
                            DebugCheck.Unreachable();
                            break;
                    }
                });
            }
            catch (Exception e)
            {
                var ccex = new CrdtpConnectionException("Failed to start reader.", e);
                Abort(ccex);
                throw ccex;
            }
        }

        private void ThrowIfInvalidState(CrdtpConnectionState expectedState)
        {
            var currentState = _state;

            if (currentState == expectedState)
            {
                if (_disposed)
                {
                    throw Error.ObjectDisposed(nameof(CrdtpConnection));
                }

                return;
            }

            throw Error.InvalidOperation($"{nameof(CrdtpConnection)} in invalid state (\"{currentState}\") for this operation. Valid states are: \"{expectedState}\".");
        }
    }
}
