#define XI_CRDTP_USE_VALUE_STRING_BUILDER

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Xilium.Crdtp.Client.Logging;

/// <summary>
/// Formats binary message to UTF-16 text.
/// </summary>
public readonly struct CrdtpUtf16MessageWriter
{
    private static readonly Encoding s_utf8EncodingLax = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

    // TODO: Add metrics, and choose proper size.
    private const int DefaultStackAllockSize = 256;

    // TODO: Make generic AbbreviateString/Buffer helper, with modes: begin, middle, end.
    public static string Format(ReadOnlySpan<byte> message,
        bool isReceive,
        in CrdtpMessageWriterOptions options)
    {
        // TODO: {"id":3,"result":{},"sessionId":"1DBB43336E601DE0F40D0EC44D3B97DB"}
        // {"method":"Target.targetDestroyed","":...,"sessionId":"5EFCCD6A7AEEE6E476C4272CEAEDE"}}
        // Bare, even non-sensible minimum length is:
        // So absolute minimum is actually about 128 length.

        if (CborHelper.IsCborMessage(message))
            throw Error.NotImplemented("Transcoding CBOR messages to text is not implemented.");

#if NET5_0_OR_GREATER
        // TODO: Add fast path (non truncated)
        // Use string.Create() when appropriate?

#if XI_CRDTP_USE_VALUE_STRING_BUILDER
        var sb = new ValueStringBuilder(stackalloc char[DefaultStackAllockSize]);
#else
        // TODO: At least use StringBuilder pool, can be called concurrently
        var sb = new StringBuilder();
#endif

        if (isReceive)
        {
            sb.Append(options.ReceivePrefix);
        }
        else
        {
            sb.Append(options.SendPrefix);
        }

        if (options.OverflowSizeInBytes <= 0
            || message.Length < options.OverflowSizeInBytes)
        {
            // Trimming: None

            // TODO: Not needed allocation - append withing buffer
            sb.Append(s_utf8EncodingLax.GetString(message));
        }
        else
        {
            var headLength = options.OverflowSizeInBytes / 2;

            // UTF-8:
            // 1 byte: 0xxxxxxx
            // 2 byte: 110xxxxx 10xxxxxx
            // 3 byte: 1110xxxx 10xxxxxx 10xxxxxx
            // 4 byte: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx

            // Skip UTF8 multibyte characters.
            for (var i = 0; i < 4 && headLength < message.Length; i++)
            {
                // Head span ends with single-byte character.
                if ((message[headLength] & 0x80) == 0)
                    break;

                // Head span ends with start of multi-byte character, but not
                // inside of multi-byte sequence.
                if ((message[headLength] & 0xC0) == 0xC0)
                    break;

                headLength++;
            }

            var tailIndex = message.Length - options.OverflowSizeInBytes / 2;
            // TODO: Adjust tailIndex, tail should not overlap with head
            if (tailIndex < headLength)
                tailIndex = headLength;

            // Skip UTF8 multibyte characters.
            for (var i = 0; i < 4 && tailIndex < message.Length; i++)
            {
                if ((message[tailIndex] & 0x80) == 0)
                    break;

                if ((message[tailIndex] & 0xC0) == 0xC0)
                    break;

                tailIndex++;
            }

            DebugCheck.That(headLength <= tailIndex);

            var tailLength = message.Length - tailIndex;

            // If result is doesn't get shorter than whole message, then just
            // use original message.
            if (headLength + tailLength + (options.EllipsisString?.Length ?? 0) >= message.Length)
            {
                sb.Append(s_utf8EncodingLax.GetString(message));
            }
            else
            {
                sb.Append(s_utf8EncodingLax.GetString(message[..headLength]));
                sb.Append(options.EllipsisString);
                sb.Append(s_utf8EncodingLax.GetString(message[tailIndex..]));
            }
        }

        return sb.ToString();
#else
        // TODO: Finish compat implementation, it can use pooled string builder
        throw new PlatformNotSupportedException("This method currently not implemented for this platform.");

        // TODO: .NET 4.5.1 can use something like
        // s_utf8EncodingLax.GetChars(bytes, message.Length, char* output, length)
        //fixed (byte* bytes = message)
        //{
        //    return s_utf8EncodingLax.GetString(bytes, message.Length);
        //}
#endif
    }
}
