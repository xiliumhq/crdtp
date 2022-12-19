using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Xilium.Crdtp.Client.Serialization;
using Xunit;

namespace Xilium.Crdtp.Client.Tests
{
    public class CrdtpSessionTests
    {
        private CrdtpSession CreateSession(string? sessionId = default)
            => new CrdtpSession(sessionId: sessionId ?? "", handler: null);

        [Fact]
        public void SessionId_MustBeNotNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CrdtpSession(null!));
        }

        [Fact]
        public void SessionId_Detached_Ok()
        {
            var session = CreateSession();
            var actual = session.SessionId;
            Assert.Equal(default, actual);
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
        public void GetNextCallId_Detached_Ok()
        {
            var session = CreateSession();
            var actual = session.GetNextCallId();
            Assert.Equal(1, actual);
        }

        [Fact]
        public void UseSerializerOptions_Detached_Ok()
        {
            var session = CreateSession();
            Assert.Throws<ArgumentNullException>(() => session.UseSerializerOptions(null!));
            session.UseSerializerOptions(TestStjSerializerOptions.Instance); // TODO: Document behavior, e.g. serializers must be registrable without being attached.
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
                    parameters: default,
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

        private sealed class TestStjSerializerOptions : StjSerializerOptions
        {
            public static readonly TestStjSerializerOptions Instance = new();

            private TestStjSerializerOptions() { }

            protected override ICollection<JsonConverter> GetConvertersCore()
                => Array.Empty<JsonConverter>();
        }
    }
}
