﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;
using Xunit;

namespace Xilium.Crdtp.Client.Tests
{
    public class CrdtpSessionTests
    {
        private sealed class FakeConnection : CrdtpConnection
        {
            public FakeConnection(CrdtpConnectionDelegate @delegate) : base(@delegate) { }

            protected override void DisposeCore() { }

            protected override Task OpenAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override Task CloseAsyncCore(CancellationToken cancellationToken)
                => Task.CompletedTask;

            protected override CrdtpConnectionReader? CreateReader()
                => null;

            protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
                => ValueTask.CompletedTask;
        }

        private CrdtpSession CreateSession(string? sessionId = default)
        {
            var client = new CrdtpClient((d) => new FakeConnection(d), handler: null);
            var session = new CrdtpSession(client, sessionId: sessionId ?? "", handler: null);
            return session;
        }

        [Fact]
        public void SessionId_MustBeNotNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new CrdtpClient((d) => new FakeConnection(d), handler: null);
                var session = new CrdtpSession(client, null!);
            });
        }

        [Fact]
        public void SessionId_Detached_Ok()
        {
            var session = CreateSession();
            var actual = session.SessionId;
            Assert.Equal("", actual);
        }

        [Fact]
        public void IsAttached_Detached_Ok()
        {
            var session = CreateSession();
            Assert.False(session.IsAttached);
        }

        [Fact]
        public void GetClient_Detached_UndecidedBehavior()
        {
            // TODO: (High) (API) What should return CrdtpSession::GetClient, when session is detached?
            // FIXME: Returning null should be ok, see implementation for more details.
            // Also consider to include client in CrdtpSession ctor.
            var session = CreateSession();
            Assert.Null(session.GetClient());
        }

        [Fact]
        public void CancelPendingRequests_Detached_Ok()
        {
            var session = CreateSession();
            session.CancelPendingRequests(); // TODO: Document behavior. This call should work regardless to session state.
        }

        [Fact]
        public void UseSerializerOptions_Detached_Ok()
        {
            var session = CreateSession();
            Assert.Throws<ArgumentNullException>(() => session.UseSerializationContextFactory(null!));
            session.UseSerializationContextFactory(TestStjSerializationContextFactory.Instance); // TODO: Document behavior, e.g. serializers must be registrable without being attached.
        }

        // TODO: Add tests for overloads
        [Fact]
        public async Task SendCommandAsync_Detached_InvalidState()
        {
            // TODO: Add similar test, to test what cancellation token checked before state.
            var session = CreateSession();
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await session.SendCommandAsync<EmptyRequest, EmptyResponse>(
                    JsonEncodedText.Encode("Test.method"),
                    parameters: default!,
                    cancellationToken: default);
            });
        }

        [Fact]
        public void AddEventHandler_Detached_Ok()
        {
            var session = CreateSession();
            session.AddEventHandler("Test.event", OnTestEvent, sender: null);

            static void OnTestEvent(object? sender, EventArgs x) { }
        }

        [Fact]
        public void AddEventHandlerT_Detached_Ok()
        {
            var session = CreateSession();
            session.AddEventHandler<EmptyEvent>("Test.event", OnTestEvent, sender: null);

            static void OnTestEvent(object? sender, EmptyEvent x) { }
        }

        [Fact]
        public void RemoveEventHandler_Detached_Ok()
        {
            var session = CreateSession();
            session.RemoveEventHandler("Test.event", OnTestEvent, sender: null);

            static void OnTestEvent(object? sender, EventArgs x) { }
        }

        [Fact]
        public void RemoveEventHandlerT_Detached_Ok()
        {
            var session = CreateSession();
            session.RemoveEventHandler<EmptyEvent>("Test.event", OnTestEvent, sender: null);

            static void OnTestEvent(object? sender, EmptyEvent x) { }
        }


        private sealed class EmptyRequest { }
        private sealed class EmptyResponse { }
        private sealed class EmptyEvent { }

        private sealed class TestStjSerializationContextFactory : StjSerializationContextFactory
        {
            public static readonly TestStjSerializationContextFactory Instance = new();

            private TestStjSerializationContextFactory() { }

            protected override JsonSerializerContext? CreateJsonSerializerContext()
                => null;

            protected override JsonConverter[] GetJsonConverters()
                => Array.Empty<JsonConverter>();
        }
    }
}
