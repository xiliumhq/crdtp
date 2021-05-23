using System;

namespace Xilium.Crdtp.Client.Logging
{
    [Obsolete("This type will be moved from next release.")]
    public class CrdtpConsoleLogger : CrdtpLogger
    {
        public override void LogReceive(ReadOnlySpan<byte> message)
        {
            // TODO(dmitry.azaraev): (High) Needs API for protocol logging (receiving).
#if NET5_0_OR_GREATER
            var text = System.Text.Encoding.UTF8.GetString(message);
#else
            var text = System.Text.Encoding.UTF8.GetString(message.ToArray());
#endif
            Console.WriteLine("< {0}", text);
        }

        public override void LogSend(ReadOnlySpan<byte> message)
        {
#if NET5_0_OR_GREATER
            var text = System.Text.Encoding.UTF8.GetString(message);
#else
            var text = System.Text.Encoding.UTF8.GetString(message.ToArray());
#endif
            Console.WriteLine("> {0}", text);
        }
    }
}
