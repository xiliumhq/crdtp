#if NET5_0_OR_GREATER
// TODO: Port to 4.x
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Xilium.Crdtp.Buffers;

namespace Xilium.Crdtp.Client;

public sealed class CrdtpPipeJsonConnection : CrdtpPipeConnection
{
    private readonly SemaphoreSlim _sendAsyncLock = new SemaphoreSlim(1, 1);
    private static readonly byte[] s_endOfMessage = new byte[1] { 0x00 };

    public CrdtpPipeJsonConnection(CrdtpConnectionDelegate handler,
        SafeFileHandle readPipe,
        SafeFileHandle writePipe)
        : base(handler, readPipe, writePipe)
    { }

    protected override ValueTask SendAsyncCore(ReadOnlyMemory<byte> message)
    {
        // FIXME: This is quick fix for issue #27.
        return SendFallbackAsync(message);

#pragma warning disable CS0162 // Unreachable code detected
        if (_sendAsyncLock.Wait(0, default))
        {
            return SendLockAcquiredAsync(message);
        }
        else
        {
            return SendFallbackAsync(message);
        }
#pragma warning restore CS0162 // Unreachable code detected
    }

    private ValueTask SendLockAcquiredAsync(ReadOnlyMemory<byte> message)
    {
        // TODO: For PipeJson we can put end of message (zero) byte directly
        // in CrdtpClient during message serialization.

        ValueTask sendTask;
        var releaseSemaphore = true;
        try
        {
            // TODO: This most likely incompatible with valuetask pooling,
            // and causes memory leak. See issue #27.
            _ = WriteStream.WriteAsync(message, cancellationToken: default);
            sendTask = WriteStream.WriteAsync(s_endOfMessage, cancellationToken: default);
            if (sendTask.IsCompleted)
            {
                return sendTask;
            }

            releaseSemaphore = false;
        }
        finally
        {
            if (releaseSemaphore)
            {
                _ = _sendAsyncLock.Release();
            }
        }

        return WaitForSendTaskAsync(sendTask);
    }

    private async ValueTask WaitForSendTaskAsync(ValueTask writeTask)
    {
        try
        {
            await writeTask.ConfigureAwait(false);
        }
        finally
        {
            _ = _sendAsyncLock.Release();
        }
    }

    private async ValueTask SendFallbackAsync(ReadOnlyMemory<byte> message)
    {
        await _sendAsyncLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var send1 = WriteStream.WriteAsync(message, cancellationToken: default);
            var send2 = WriteStream.WriteAsync(s_endOfMessage, cancellationToken: default);

            await send1.ConfigureAwait(false);
            await send2.ConfigureAwait(false);
        }
        finally
        {
            _ = _sendAsyncLock.Release();
        }
    }

    protected override CrdtpConnectionReader? CreateReader()
        => new Reader(this);

    private sealed class Reader : CrdtpConnectionReader
    {
        private readonly CrdtpPipeJsonConnection _connection;

        public Reader(CrdtpPipeJsonConnection connection)
        {
            _connection = connection;
        }

        protected override async Task RunReadLoopAsync(CancellationToken cancellationToken)
        {
            var handler = _connection.Delegate;

            // TODO: Add metric to maximum buffer size reached.
            var ioBuffer = new CrdtpArrayBufferWriter<byte>(4096);

            var wOffset = 0;
            var chunkOffset = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                // Read message into buffer.
                var readToMemory = ioBuffer.GetMemory();
                var bytesRead = await _connection.ReadStream
                    .ReadAsync(readToMemory)
                    .ConfigureAwait(false);
                if (bytesRead == 0)
                    break;
                ioBuffer.Advance(bytesRead);

                // Scan last read chunk for end of message.
                if (bytesRead == 1 && readToMemory.Span[0] == 0)
                {
                    // TODO: Not sure if this block really necessary / useful.
                    // (e.g. other branch basically doing same thing)
                    var wEndOffset = chunkOffset;
                    var wLength = wEndOffset - wOffset;
                    DebugCheck.That(wLength > 0);
                    handler.OnMessage(
                        ioBuffer.WrittenSpan.Slice(wOffset, wLength)
                        );
                    wOffset = wEndOffset + 1;
                }
                else
                {
                    var offset = 0; // relative to last read chunk
                    while (offset < bytesRead)
                    {
                        var pos = readToMemory.Span
                            .Slice(offset, bytesRead - offset)
                            .IndexOf((byte)0);
                        if (pos >= 0)
                        {
                            var wEndOffset = chunkOffset + offset + pos;
                            var wLength = wEndOffset - wOffset;

                            DebugCheck.That(wLength > 0);
                            handler.OnMessage(
                                ioBuffer.WrittenSpan.Slice(wOffset, wLength)
                                );
                            offset = offset + pos + 1;
                            wOffset = wEndOffset + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // Index of current chunk in WrittenSpan.
                chunkOffset += bytesRead;

                // Whole buffer is consumed, so it can be discarded, without copying.
                if (wOffset == ioBuffer.WrittenSpan.Length)
                {
                    ioBuffer.Clear();
                    wOffset = 0;
                    chunkOffset = 0;
                }
                else
                {
                    // TODO: Handle case when there is a lot of already consumed data,
                    // but we are in the middle of message. In such case, 
                    // there is have sense to trim/move excess data. Can be implemented
                    // by copying to buffer writer & swap them, or by extending
                    // CrdtpArrayBufferWriter directly. (Standard ArrayBufferWriter
                    // doesn't have consuming concept.)
                }
            }

            // Ensure what we consume all data.
            if (ioBuffer.WrittenCount > 0)
            {
                throw new InvalidOperationException("Message framing error.");
            }
        }
    }
}
#endif
