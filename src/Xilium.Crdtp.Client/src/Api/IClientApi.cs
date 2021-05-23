using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xilium.Crdtp.Client.Serialization;

namespace Xilium.Crdtp.Client.Api
{
    // TODO: Remove API interfaces when they are no more needed.

    internal interface IClientApi : IDisposable // TODO: IAsyncDisposable -> same as CloseAsync but did not throw exception.
    {
        CrdtpClientState State { get; }

        CrdtpEncoding Encoding { get; }

        Task OpenAsync(CancellationToken cancellationToken = default);
        Task CloseAsync(CancellationToken cancellationToken = default);
        // void Close();
        void Abort(Exception? exception);

        void Attach(CrdtpSession session);
        // bool TryAttach(CrdtpSession session);
        void Detach(CrdtpSession session);
        // bool TryDetach(CrdtpSession session);

        bool TryGetSession(string? sessionId, [NotNullWhen(true)] out CrdtpSession? session);

    }
}
