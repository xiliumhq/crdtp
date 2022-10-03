#if XI_CRDTP_USE_INTERNAL_API_INTERFACES
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Client
{
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
#endif
