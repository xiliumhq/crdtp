#if NET5_0_OR_GREATER
// TODO: Port to 4.x
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Xilium.Crdtp.Client;

public abstract class CrdtpPipeConnection : CrdtpConnection
{
    protected readonly FileStream WriteStream;
    protected readonly FileStream ReadStream;

    public CrdtpPipeConnection(CrdtpConnectionDelegate handler,
        SafeFileHandle readPipe,
        SafeFileHandle writePipe)
        : base(handler)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(readPipe);
        ArgumentNullException.ThrowIfNull(writePipe);
#else
        Check.Argument.NotNull(readPipe, nameof(readPipe));
        Check.Argument.NotNull(writePipe, nameof(writePipe));
#endif
        Check.That(!readPipe.IsInvalid && !writePipe.IsInvalid);

        // TODO: This streams are in unbuffered mode, which generally exactly we
        // are want, but currently sending requires 2 calls + synchronization.
        // So, play with buffered mode, if it gain any win.
        // TODO: Add to CrdtpClient serialization mode when message will include
        // ending zero, so message can be issued with single call.
        ReadStream = new FileStream(readPipe, FileAccess.Read, bufferSize: 0, isAsync: true);
        WriteStream = new FileStream(writePipe, FileAccess.Write, bufferSize: 0, isAsync: true);
    }

    protected override void DisposeCore()
    {
        ReadStream?.Dispose();
        WriteStream?.Dispose();
    }

    protected override Task OpenAsyncCore(CancellationToken cancellationToken)
    {
        // TODO: No-op, but can ensure pipe validity.
        return Task.CompletedTask;
    }

    protected override Task CloseAsyncCore(CancellationToken cancellationToken)
    {
        // TODO: No-op, but can close handles.
        return Task.CompletedTask;
    }
}
#endif
