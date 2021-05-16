using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Medium) Not sure what CrdtpConnection really neededs IAsyncDisposable.
    public abstract partial class CrdtpConnection : IAsyncDisposable, IDisposable
    {
        protected readonly CrdtpConnectionDelegate Delegate;
        private readonly object StateUpdateLock = new object();

        private CrdtpConnectionState _state = CrdtpConnectionState.None;
        private CrdtpConnectionReader? _reader;

        protected CrdtpConnection(CrdtpConnectionDelegate @delegate)
        {
            Check.Argument.NotNull(@delegate, nameof(@delegate));
            Delegate = @delegate;
        }

        #region Disposable

        // TODO: Review CrdtpConnection::Disposable implementation.

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="CrdtpConnection"/>.
        /// </summary>
        /// <param name="disposing">If true, the <see cref="CrdtpConnection"/> is being disposed. If false, the <see cref="CrdtpConnection"/> is being finalized.</param>
        protected virtual void Dispose(bool disposing)
        {
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

        #endregion

        public CrdtpConnectionState State => _state;

        public virtual CrdtpEncoding Encoding => CrdtpEncoding.Json;

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            SetState(CrdtpConnectionState.Connecting, CrdtpConnectionState.None);

            try
            {
                await OpenAsyncCore(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                SetStateNoValidate(CrdtpConnectionState.Closed);
                Dispose();
                throw;
            }

            SetState(CrdtpConnectionState.Connected, CrdtpConnectionState.Connecting);
            Delegate.OnConnect(); // TODO(dmitry.azaraev): CrdtpConnection: OnConnected and make it last?

            var reader = _reader = CreateReader();

            if (reader != null)
            {
                // TODO: Start reader
                reader.RunAsync();
                reader.Task.ContinueWith((t) =>
                {
                    Console.WriteLine("Reader Faulted: {0}", t.Exception);
                    // TODO(dmitry.azaraev): CrdtpConnection: abort connection
                }, TaskContinuationOptions.OnlyOnFaulted);
            }

            SetState(CrdtpConnectionState.Open, CrdtpConnectionState.Connected);
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            SetState(CrdtpConnectionState.Closing, CrdtpConnectionState.Open);

            try
            {
                await CloseAsyncCore(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                SetStateNoValidate(CrdtpConnectionState.Aborted);
                Dispose();
                throw;
            }

            SetState(CrdtpConnectionState.Closed, CrdtpConnectionState.Closing);
        }

        // TODO(dmitry.azaraev): CrdtpConnection::SendAsync methods should not support cancellation by design (as we doesn't want making any token linking anyway).
        public ValueTask SendAsync(byte[] message, CancellationToken cancellationToken = default)
            => SendAsync(new ReadOnlyMemory<byte>(message), cancellationToken);

        public ValueTask SendAsync(ArraySegment<byte> message, CancellationToken cancellationToken = default)
            => SendAsync(message.AsMemory(), cancellationToken);

        public ValueTask SendAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default)
            => SendAsyncCore(message, cancellationToken);

        protected abstract Task OpenAsyncCore(CancellationToken cancellationToken);

        protected abstract Task CloseAsyncCore(CancellationToken cancellationToken);

        protected abstract ValueTask SendAsyncCore(ReadOnlyMemory<byte> message, CancellationToken cancellationToken);

        protected abstract CrdtpConnectionReader? CreateReader();

        // TODO(dmitry.azaraev): (High) CrdtpConnection: Redesign state transitions and checks...

        private void SetState(CrdtpConnectionState newState, CrdtpConnectionState validState)
        {
            lock (StateUpdateLock)
            {
                var currentState = _state;
                if (currentState != validState)
                {
                    // TODO: Use CrdtpException
                    throw Error.InvalidOperation("CrdtpConnection in invalid state. Current state: {0} Valid states: {1}", currentState, validState);
                }

                if (currentState != newState)
                {
                    // TODO: Invoke handler out of StateUpdateLock
                    switch (newState)
                    {
                        case CrdtpConnectionState.Closed:
                        case CrdtpConnectionState.Aborted:
                            Delegate.OnClose();
                            break;
                    }
                }

                _state = newState;
            }
        }

        private void SetStateNoValidate(CrdtpConnectionState newState)
        {
            lock (StateUpdateLock)
            {
                _state = newState;
            }
        }

        //protected void SetState(CrdtpConnectionState state)
        //{
        //    lock (StateUpdateLock)
        //    {
        //        _state = state;
        //    }
        //}

        //protected void ThrowIfInvalidState(CrdtpConnectionState validState1)
        //{
        //    var currentState = _state;
        //    if (currentState != validState1)
        //    {
        //        // TODO: Use proper exception
        //        throw Error.InvalidOperation("CrdtpConnection in invalid state. Current state: {0} Valid states: {1}", currentState, validState1);
        //    }
        //}
    }
}
