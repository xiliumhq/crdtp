using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;

namespace Xilium.Crdtp.Client
{
    public sealed partial class CrdtpClient : IAsyncDisposable, IDisposable
    {
        private static readonly StjJsonSerializerOptionsBuilder s_jsonSerializerOptionsBuilder = CreateJsonSerializerOptionsBuilder();

        private readonly CrdtpConnection _connection;
        private readonly CrdtpEncoding _encoding;

        private CrdtpSession? _defaultSession;
        private readonly ConcurrentDictionary<string, CrdtpSession> _sessions; // TODO(dmitry.azaraev): (Low) Don't use ConcurrentDictionary.

        public CrdtpClient(Func<CrdtpConnectionDelegate, CrdtpConnection> connectionFactory)
        {
            var handler = new ConnectionDelegate(this);
            _connection = connectionFactory(handler);
            _encoding = _connection.Encoding;
            _sessions = new ConcurrentDictionary<string, CrdtpSession>();
        }

        #region IDisposable

        // TODO: Review CrdtpClient::IDisposable implementation. Need tests.

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
        /// Disposes the <see cref="Connection"/>.
        /// </summary>
        /// <param name="disposing">If true, the <see cref="Connection"/> is being disposed. If false, the <see cref="Connection"/> is being finalized.</param>
        private void Dispose(bool disposing)
        {
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

        #endregion

        internal CrdtpEncoding Encoding => _encoding;

        public Task OpenAsync(CancellationToken cancellationToken = default)
            => _connection.OpenAsync(cancellationToken);

        public Task CloseAsync(CancellationToken cancellationToken = default)
            => _connection.CloseAsync(cancellationToken);

        public CrdtpSession CreateSession(string? sessionId) // TODO: doesn't like it too much actually.
        {
            var session = new CrdtpSession(this, sessionId);
            Attach(session);
            return session;
        }

        public TSession CreateSession<TSession>(string? sessionId, Func<CrdtpSession, TSession> sessionFactory)
            => sessionFactory(CreateSession(sessionId));

        internal void Attach(CrdtpSession session)
        {
            var sessionId = session.SessionId;

            if (string.IsNullOrEmpty(sessionId))
            {
                if (Interlocked.CompareExchange(ref _defaultSession, session, null) == null)
                {
                    session.OnAttached();
                    return;
                }
                else throw Error.InvalidOperation("Another default session already attached.");
            }
            else
            {
                if (_sessions.TryAdd(sessionId, session))
                {
                    session.OnAttached();
                }
                else throw Error.InvalidOperation("Session with same SessionId already attached.");
            }
        }

        public void Detach(CrdtpSession session)
        {
            throw Error.NotImplemented();
        }

        public bool TryGetSession(string? sessionId, [NotNullWhen(true)] out CrdtpSession? session)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                session = _defaultSession;
                return session != null;
            }
            else
            {
                return _sessions.TryGetValue(sessionId, out session);
            }
        }

        internal ValueTask SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            NotifySend(message);
            return _connection.SendAsync(message, cancellationToken);
        }

        internal ValueTask SendAsync(ArraySegment<byte> message, CancellationToken cancellationToken)
        {
            NotifySend(message.AsSpan());
            return _connection.SendAsync(message, cancellationToken);
        }

        internal ValueTask SendAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            NotifySend(message.Span);
            return _connection.SendAsync(message, cancellationToken);
        }

        private void NotifySend(ReadOnlySpan<byte> message)
        {
            if (CrdtpFeatures.IsLogSendEnabled)
            {
                // TODO(dmitry.azaraev): (High) Needs API for protocol logging (sending).
#if NET5_0_OR_GREATER
                var text = System.Text.Encoding.UTF8.GetString(message);
#else
                var text = System.Text.Encoding.UTF8.GetString(message.ToArray());
#endif
                Console.WriteLine("> {0}", text);
            }
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
    }
}
