// #define XI_CRDTPCLIENT_ASYNCDISPOSABLE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;

namespace Xilium.Crdtp.Client
{
    public sealed partial class CrdtpClient : IDisposable
#if XI_CRDTPCLIENT_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
    {
        private static readonly StjJsonSerializerOptionsBuilder s_jsonSerializerOptionsBuilder = CreateJsonSerializerOptionsBuilder();

        private readonly CrdtpConnection _connection;
        private readonly CrdtpEncoding _encoding;
        private readonly CrdtpLogger? _logger;
        private readonly CrdtpClientHandler? _handler;

        private readonly object StateAndSessionMapLock = new object();
        private CrdtpClientState _state;
        private bool _disposed;
        private CrdtpSession? _defaultSession;
        private readonly Dictionary<string, CrdtpSession> _sessions;

        public CrdtpClient(Func<CrdtpConnectionDelegate, CrdtpConnection> connectionFactory,
            CrdtpClientHandler? handler = null,
            CrdtpLogger? logger = null)
        {
            _connection = connectionFactory(new ConnectionDelegate(this));
            _handler = handler;
            _logger = logger;
            _encoding = _connection.Encoding;
            _sessions = new Dictionary<string, CrdtpSession>();
        }

        public CrdtpClientState State => _state;

        #region IDisposable

        // TODO: Review CrdtpClient::IDisposable implementation. Need tests.

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="CrdtpClient"/>.
        /// </summary>
        /// <param name="disposing">If true, the <see cref="CrdtpClient"/> is being disposed. If false, the <see cref="CrdtpClient"/> is being finalized.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (StateAndSessionMapLock)
                {
                    if (!_disposed)
                    {
                        _disposed = true;

                        _connection.Dispose();

                        // TODO: Instead of calling DisposeSessions, don't do that.
                        // Connection already are notifies us, so calling top-level
                        // Dispose method is not intended.
                        DisposeSessions();

                        DebugCheck.That(_state >= CrdtpClientState.Closed);
                        DebugCheck.That(_defaultSession == null);
                        DebugCheck.That(_sessions.Count == 0);
                    }
                }
            }
        }

        private void DisposeSessions()
        {
            DebugCheck.That(Monitor.IsEntered(StateAndSessionMapLock), $"{nameof(StateAndSessionMapLock)} should be held.");

            var defaultSession = _defaultSession;
            if (defaultSession != null)
            {
                _defaultSession = null;
                lock (defaultSession.StateAndRequestMapLock)
                {
                    defaultSession.DetachInternal();
                }
            }

            // TODO: here we may swap session lists, because they will automatically unregistered...
            foreach (var session in _sessions.Values)
            {
                lock (session.StateAndRequestMapLock)
                {
                    session.DetachInternal();
                }
            }

            _sessions.Clear();
        }

        #endregion

        #region IAsyncDisposable
#if XI_CRDTPCLIENT_ASYNCDISPOSABLE

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the <see cref="Connection"/>.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        private ValueTask DisposeAsyncCore()
        {
            Dispose(true);
            return default;
        }

#endif
        #endregion

        public Task OpenAsync(CancellationToken cancellationToken = default)
            => _connection.OpenAsync(cancellationToken);

        public Task CloseAsync(CancellationToken cancellationToken = default)
            => _connection.CloseAsync(cancellationToken);

        public void Abort(Exception? exception)
            => _connection.Abort(exception);

        public CrdtpEncoding Encoding => _encoding;

        // TODO: Rename to AttachSession?
        [Obsolete("Avoid this call.")]
        public CrdtpSession CreateSession(string? sessionId)
        {
            var session = new CrdtpSession(sessionId);
            Attach(session);
            return session;
        }

        // TODO: Rename to AttachSession?
        [Obsolete("Avoid this call.")]
        public TProxy CreateSession<TProxy>(string? sessionId, Func<CrdtpSession, TProxy> proxyFactory)
            => proxyFactory(CreateSession(sessionId));

        public void Attach(CrdtpSession session)
        {
            lock (StateAndSessionMapLock)
            {
                // TODO: validate state, attach only on None and Open states.

                lock (session.StateAndRequestMapLock)
                {
                    if (session.IsAttached) throw Error.InvalidOperation("Given session already attached.");

                    var client = session.GetClientOrDefault();
                    if (client != null) throw Error.InvalidOperation("Given session already attached.");

                    lock (session.EventDispatcherMapLock)
                    {
                        AddSessionOrThrow(session);
                        session.AttachToInternal(this);
                    }
                }
            }
        }

        public void Detach(CrdtpSession session)
        {
            lock (StateAndSessionMapLock)
            {
                // TODO: validate state, detach only on None and Open states.

                lock (session.StateAndRequestMapLock)
                {
                    if (!session.IsAttached) throw Error.InvalidOperation("Given session is not attached.");

                    if (TryRemoveSession(session))
                    {
                        session.DetachInternal();
                    }
                }
            }
        }

        private void AddSessionOrThrow(CrdtpSession session)
        {
            DebugCheck.That(Monitor.IsEntered(StateAndSessionMapLock), $"{nameof(StateAndSessionMapLock)} should be held.");

            var sessionId = session.SessionId;

            if (string.IsNullOrEmpty(sessionId))
            {
                // TODO: We already under lock
                if (Interlocked.CompareExchange(ref _defaultSession, session, null) == null)
                {
                    // Succeed.
                    return;
                }
                else throw Error.InvalidOperation("Another default session already attached.");
            }
            else
            {
                // TODO: take lock, and move out from concurrent dictionary.
                if (_sessions.TryAdd(sessionId, session))
                {
                    // Succeed.
                    return;
                }
                else throw Error.InvalidOperation("Session with same SessionId already attached.");
            }
        }

        // This method should not throw exception.
        internal bool TryRemoveSession(CrdtpSession session)
        {
            DebugCheck.That(Monitor.IsEntered(StateAndSessionMapLock), $"{nameof(StateAndSessionMapLock)} should be held.");

            var sessionId = session.SessionId;

            if (string.IsNullOrEmpty(sessionId))
            {
                return Interlocked.CompareExchange(ref _defaultSession, null, session) == session;
            }
            else
            {
                // TODO: .net 5 Remove(key, out var sessionToRemove)

                // TODO: take lock, and move out from concurrent dictionary.
                if (_sessions.TryGetValue(sessionId, out var sessionToRemove))
                {
                    if ((object)session == sessionToRemove)
                    {
                        return _sessions.Remove(sessionId);
                    }
                }

                return false;
            }
        }

        public bool TryGetSession(string? sessionId, [NotNullWhen(true)] out CrdtpSession? session)
        {
            // TODO: Needs take a lock
            if (string.IsNullOrEmpty(sessionId))
            {
                session = _defaultSession; // TODO: Volatile.Read?
                return session != null;
            }
            else
            {
                lock (StateAndSessionMapLock)
                {
                    return _sessions.TryGetValue(sessionId, out session);
                }
            }
        }

        internal ValueTask SendAsync(ReadOnlyMemory<byte> message)
        {
            _logger?.LogSend(message.Span);
            return _connection.SendAsync(message);
        }

        // TODO(dmitry.azaraev): (Low) AggressiveInline
        internal static StjJsonSerializerOptionsBuilder JsonSerializerOptionsBuilder
            => s_jsonSerializerOptionsBuilder;

        private static StjJsonSerializerOptionsBuilder CreateJsonSerializerOptionsBuilder()
        {
            var provider = new StjJsonSerializerOptionsBuilder(DefaultStjSerializerOptions.CreateJsonSerializerOptions());
            provider.Add(new DefaultStjSerializerOptions());
            return provider;
        }

        internal void OnOpen()
        {
            lock (StateAndSessionMapLock)
            {
                Check.That(_state == CrdtpClientState.None);
                _state = CrdtpClientState.Open;
                _handler?.OnOpen();
            }
        }

        internal void OnClose()
        {
            lock (StateAndSessionMapLock)
            {
                if (_state < CrdtpClientState.Closed)
                {
                    _state = CrdtpClientState.Closed;
                    _handler?.OnClose();
                    Dispose();
                }
            }
        }

        internal void OnAbort(Exception? exception)
        {
            lock (StateAndSessionMapLock)
            {
                if (_state < CrdtpClientState.Closed)
                {
                    _state = CrdtpClientState.Aborted;
                    _handler?.OnAbort(exception);
                    Dispose();
                }
            }
        }
    }
}
